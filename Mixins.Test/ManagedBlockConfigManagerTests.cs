using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Moq;
using NUnit.Framework;
using Shouldly;

using Sandbox.ModAPI;

using static IngameScript.Program;

using Mixins.Test.Common;

namespace Mixins.Test
{
    [TestFixture]
    public class ManagedBlockConfigManagerTests
    {
        #region Test Context

        public class TestContext
        {
            public TestContext()
            {
                BlockName = "blockName";
                BlockTag = "blockTag";

                MockLogger = new Mock<ILogger>();

                _mockManagedBlockConfigParseHandlers = new List<Mock<IManagedBlockConfigParseHandler>>();
                _managedBlockConfigParseHandlers = new List<IManagedBlockConfigParseHandler>();

                MockManagerSettingsProvider = new Mock<IManagerSettingsProvider>();
                MockManagerSettingsProvider
                    .Setup(x => x.Settings)
                    .Returns(() => new ManagerSettings()
                    {
                        BlockTag = BlockTag
                    });

                Uut = new ManagedBlockConfigManager(
                    MockLogger.Object,
                    _managedBlockConfigParseHandlers,
                    MockManagerSettingsProvider.Object);

                MockBlock = new Mock<IMyTerminalBlock>();
                MockBlock
                    .Setup(x => x.CustomName)
                    .Returns(() => BlockName);
                MockBlock
                    .Setup(x => x.CustomData)
                    .Returns(() => Config);

                MockBackgroundWorker = new FakeBackgroundWorker();
            }

            public string BlockTag;

            public readonly Mock<ILogger> MockLogger;

            public IReadOnlyList<Mock<IManagedBlockConfigParseHandler>> MockManagedBlockConfigParseHandlers
                => _mockManagedBlockConfigParseHandlers;

            public readonly Mock<IManagerSettingsProvider> MockManagerSettingsProvider;

            public readonly ManagedBlockConfigManager Uut;

            public string BlockName;

            public string Config;

            public readonly Mock<IMyTerminalBlock> MockBlock;

            public readonly FakeBackgroundWorker MockBackgroundWorker;

            public Mock<IManagedBlockConfigParseHandler> AddFakeManagedBlockConfigParseHandler(ParseResult parseResult)
            {
                var mockManagedBlockConfigParseHandler = new Mock<IManagedBlockConfigParseHandler>();

                mockManagedBlockConfigParseHandler
                    .Setup(x => x.OnParsing(It.IsAny<ManagedBlockConfigLine>()))
                    .Returns(parseResult);

                _mockManagedBlockConfigParseHandlers.Add(mockManagedBlockConfigParseHandler);
                _managedBlockConfigParseHandlers.Add(mockManagedBlockConfigParseHandler.Object);

                return mockManagedBlockConfigParseHandler;
            }

            private List<Mock<IManagedBlockConfigParseHandler>> _mockManagedBlockConfigParseHandlers;

            private List<IManagedBlockConfigParseHandler> _managedBlockConfigParseHandlers;
        }

        #endregion Test Context

        #region Test Cases

        public static readonly string[][][] ValidConfigDatas
            = new[]
            {
                new[] { new[] { "blockTag1", "option1"                     }                                                                     },
                new[] { new[] { "blockTag2", "option2", "param1"           }                                                                     },
                new[] { new[] { "blockTag3", "option3", "param2", "param3" }                                                                     },
                new[] { new[] { "blockTag4", "option4"                     }, new[] { "blockTag4", "option5" }                                   },
                new[] { new[] { "blockTag5", "option6"                     }, new[] { "blockTag5", "option7" }, new[] { "blockTag5", "option8" } },
            };

        public static readonly string[][][] InvalidConfigDatas
            = new[]
            {
                new[] { new[] { "blockTag6" }                                               },
                new[] { new[] { "blockTag8" }, new[] { "blockTag8" }                        },
                new[] { new[] { "blockTag8" }, new[] { "blockTag8" }, new[] { "blockTag8" } },
            };

        public static readonly TestCaseData[] ValidBlockTagAndConfigDataTestCases
            = ValidConfigDatas
                .Select(configData => new TestCaseData(new object[] { configData[0][0], configData })
                    .SetName($"{{m}}({{0}}, \"{Regex.Escape(BuildConfig(configData))}\")"))
                .ToArray();

        public static readonly TestCaseData[] WrongBlockTagAndValidConfigDataTestCases
            = ValidConfigDatas
                .Select(configData => new TestCaseData(new object[] { "wrongBlockTag", configData })
                    .SetName($"{{m}}({{0}}, \"{Regex.Escape(BuildConfig(configData))}\")"))
                .ToArray();

        public static readonly TestCaseData[] ValidBlockTagAndInvalidConfigDataTestCases
            = InvalidConfigDatas
                .Select(configData => new TestCaseData(new object[] { configData[0][0], configData })
                    .SetName($"{{m}}({{0}}, \"{Regex.Escape(BuildConfig(configData))}\")"))
                .ToArray();

        public static string BuildConfig(string[][] configData)
            => string.Join("\n", configData
                .Select(BuildConfigLine));

        public static string BuildConfigLine(string[] configParams)
            => string.Join(":", configParams);

        #endregion Test Cases

        #region MakeParseOperation() Tests

        [TestCaseSource(nameof(ValidBlockTagAndConfigDataTestCases))]
        public void MakeParseOperation_ManagedBlockConfigParseHandlersIsEmpty_CompletesImmediately(string blockTag, string[][] configData)
        {
            var testContext = new TestContext()
            {
                BlockTag = blockTag,
                Config = BuildConfig(configData)
            };

            testContext.Uut.MakeParseOperation(testContext.MockBlock.Object)
                .ShouldRunToCompletion(testContext.MockBackgroundWorker);

            testContext.MockLogger.ShouldNotHaveReceived(x => x.AddLine(It.IsAny<string>()));
        }

        [TestCase("")]
        [TestCase(":")]
        [TestCase("\n")]
        [TestCase(":\n:\n:")]
        public void MakeParseOperation_ConfigIsEmptyOrWhitespace_CompletesAfterStarting(string config)
        {
            var testContext = new TestContext()
            {
                Config = config
            };
            testContext.AddFakeManagedBlockConfigParseHandler(ParseResult.Ignored);

            testContext.Uut.MakeParseOperation(testContext.MockBlock.Object)
                .ShouldRunToCompletion(testContext.MockBackgroundWorker);

            foreach (var mockManagedBlockConfigParseHandler in testContext.MockManagedBlockConfigParseHandlers)
            {
                mockManagedBlockConfigParseHandler.ShouldHaveReceived(x => x.OnStarting(), 1);
                mockManagedBlockConfigParseHandler.ShouldNotHaveReceived(x => x.OnParsing(It.IsAny<ManagedBlockConfigLine>()));
                mockManagedBlockConfigParseHandler.ShouldHaveReceived(x => x.OnCompleted(), 1);
            }

            testContext.MockLogger.ShouldNotHaveReceived(x => x.AddLine(It.IsAny<string>()));
        }

        [TestCaseSource(nameof(WrongBlockTagAndValidConfigDataTestCases))]
        public void MakeParseOperation_BlockTagIsWrong_IgnoresLine(string blockTag, string[][] configData)
        {
            var testContext = new TestContext()
            {
                BlockTag = blockTag,
                Config = BuildConfig(configData)
            };
            testContext.AddFakeManagedBlockConfigParseHandler(ParseResult.Ignored);

            testContext.Uut.MakeParseOperation(testContext.MockBlock.Object)
                .ShouldRunToCompletion(testContext.MockBackgroundWorker);

            foreach (var mockManagedBlockConfigParseHandler in testContext.MockManagedBlockConfigParseHandlers)
            {
                mockManagedBlockConfigParseHandler.ShouldHaveReceived(x => x.OnStarting(), 1);
                mockManagedBlockConfigParseHandler.ShouldNotHaveReceived(x => x.OnParsing(It.IsAny<ManagedBlockConfigLine>()));
                mockManagedBlockConfigParseHandler.ShouldHaveReceived(x => x.OnCompleted(), 1);
            }

            testContext.MockLogger.ShouldNotHaveReceived(x => x.AddLine(It.IsAny<string>()));
        }

        [TestCaseSource(nameof(ValidBlockTagAndInvalidConfigDataTestCases))]
        public void MakeParseOperation_ConfigLineIsInvalid_LogsError(string blockTag, string[][] configData)
        {
            var testContext = new TestContext()
            {
                BlockTag = blockTag,
                Config = BuildConfig(configData)
            };
            testContext.AddFakeManagedBlockConfigParseHandler(ParseResult.Ignored);

            testContext.Uut.MakeParseOperation(testContext.MockBlock.Object)
                .ShouldRunToCompletion(testContext.MockBackgroundWorker);

            foreach (var mockManagedBlockConfigParseHandler in testContext.MockManagedBlockConfigParseHandlers)
            {
                mockManagedBlockConfigParseHandler.ShouldHaveReceived(x => x.OnStarting(), 1);
                mockManagedBlockConfigParseHandler.ShouldNotHaveReceived(x => x.OnParsing(It.IsAny<ManagedBlockConfigLine>()));
                mockManagedBlockConfigParseHandler.ShouldHaveReceived(x => x.OnCompleted(), 1);
            }

            foreach (var configLineData in configData)
            {
                var configLine = BuildConfigLine(configLineData);

                testContext.MockLogger.ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.Contains(configLine))));
            }
        }

        [TestCaseSource(nameof(ValidBlockTagAndConfigDataTestCases))]
        public void MakeParseOperation_Always_SplitsLinesAndParamsCorrectly(string blockTag, string[][] configData)
        {
            var testContext = new TestContext()
            {
                BlockTag = blockTag,
                Config = BuildConfig(configData)
            };
            var mockManagedBlockConfigParseHandler = testContext.AddFakeManagedBlockConfigParseHandler(ParseResult.Success);

            var configLines = new List<ManagedBlockConfigLine>();

            mockManagedBlockConfigParseHandler
                .Setup(x => x.OnParsing(It.IsAny<ManagedBlockConfigLine>()))
                .Callback<ManagedBlockConfigLine>(x => configLines.Add(x))
                .Returns(ParseResult.Success);

            testContext.Uut.MakeParseOperation(testContext.MockBlock.Object)
                .ShouldRunToCompletion(testContext.MockBackgroundWorker);

            configLines.Count.ShouldBe(configData.Length);
            foreach(var i in Enumerable.Range(0, configLines.Count))
            {
                var expectedConfigLine = new ManagedBlockConfigLine(configData[i]);

                configLines[i].BlockTag.ShouldBe(expectedConfigLine.BlockTag);
                configLines[i].Option.ShouldBe(expectedConfigLine.Option);
                configLines[i].ParamCount.ShouldBe(expectedConfigLine.ParamCount);

                foreach (var index in Enumerable.Range(0, configLines[i].ParamCount))
                    configLines[i].GetParam(index).ShouldBe(expectedConfigLine.GetParam(index));
            }
        }

        [TestCaseSource(nameof(ValidBlockTagAndConfigDataTestCases))]
        public void MakeParseOperation_LineIsIgnored_LogsError(string blockTag, string[][] configData)
        {
            var testContext = new TestContext()
            {
                BlockTag = blockTag,
                Config = BuildConfig(configData)
            };
            foreach (var _ in Enumerable.Repeat(0, 3))
                testContext.AddFakeManagedBlockConfigParseHandler(ParseResult.Ignored);

            testContext.Uut.MakeParseOperation(testContext.MockBlock.Object)
                .ShouldRunToCompletion(testContext.MockBackgroundWorker);

            foreach (var configLineData in configData)
            {
                foreach (var mockManagedBlockConfigParseHandler in testContext.MockManagedBlockConfigParseHandlers)
                {
                    mockManagedBlockConfigParseHandler.ShouldHaveReceived(x => x.OnStarting(), 1);
                    mockManagedBlockConfigParseHandler.ShouldHaveReceivedOnParsing(new ManagedBlockConfigLine(configLineData));
                    mockManagedBlockConfigParseHandler.ShouldHaveReceived(x => x.OnCompleted(), 1);
                }

                testContext.MockLogger.ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.Contains(BuildConfigLine(configLineData)))));
            }
        }

        [TestCaseSource(nameof(ValidBlockTagAndConfigDataTestCases))]
        public void MakeParseOperation_LineParseIsError_LogsErrorAndEndsLineParse(string blockTag, string[][] configData)
        {
            var testContext = new TestContext()
            {
                BlockTag = blockTag,
                Config = BuildConfig(configData)
            };

            var errorText = "errorText";
            foreach (var handlerIndex in Enumerable.Repeat(0, 3))
                testContext.AddFakeManagedBlockConfigParseHandler(ParseResult.FromError($"{errorText}{handlerIndex}"));

            testContext.Uut.MakeParseOperation(testContext.MockBlock.Object)
                .ShouldRunToCompletion(testContext.MockBackgroundWorker);

            foreach (var configLineData in configData)
            {
                foreach (var mockManagedBlockConfigParseHandler in testContext.MockManagedBlockConfigParseHandlers)
                {
                    mockManagedBlockConfigParseHandler.ShouldHaveReceived(x => x.OnStarting(), 1);
                    mockManagedBlockConfigParseHandler.ShouldHaveReceived(x => x.OnCompleted(), 1);
                }

                testContext.MockManagedBlockConfigParseHandlers[0]
                    .ShouldHaveReceivedOnParsing(new ManagedBlockConfigLine(configLineData));

                foreach (var mockManagedBlockConfigParseHandler in testContext.MockManagedBlockConfigParseHandlers.Skip(1))
                    mockManagedBlockConfigParseHandler.ShouldNotHaveReceived(x => x.OnParsing(It.IsAny<ManagedBlockConfigLine>()));

                testContext.MockLogger.ShouldHaveReceived(x => x.AddLine(It.Is<string>(y =>
                    y.Contains(BuildConfigLine(configLineData))
                        && y.Contains($"{errorText}0"))));

                foreach(var i in Enumerable.Range(0, testContext.MockManagedBlockConfigParseHandlers.Count).Skip(1))
                    testContext.MockLogger.ShouldNotHaveReceived(x => x.AddLine(It.Is<string>(y =>
                        y.Contains(BuildConfigLine(configLineData))
                            && y.Contains($"{errorText}{i}"))));
            }
        }

        [TestCaseSource(nameof(ValidBlockTagAndConfigDataTestCases))]
        public void MakeParseOperation_LineParseIsSuccessful_EndsLineParse(string blockTag, string[][] configData)
        {
            var testContext = new TestContext()
            {
                BlockTag = blockTag,
                Config = BuildConfig(configData)
            };

            foreach (var handlerIndex in Enumerable.Repeat(0, 3))
                testContext.AddFakeManagedBlockConfigParseHandler(ParseResult.Success);

            testContext.Uut.MakeParseOperation(testContext.MockBlock.Object)
                .ShouldRunToCompletion(testContext.MockBackgroundWorker);

            foreach (var configLineData in configData)
            {
                foreach (var mockManagedBlockConfigParseHandler in testContext.MockManagedBlockConfigParseHandlers)
                {
                    mockManagedBlockConfigParseHandler.ShouldHaveReceived(x => x.OnStarting(), 1);
                    mockManagedBlockConfigParseHandler.ShouldHaveReceived(x => x.OnCompleted(), 1);
                }

                testContext.MockManagedBlockConfigParseHandlers[0]
                    .ShouldHaveReceivedOnParsing(new ManagedBlockConfigLine(configLineData));

                foreach (var mockManagedBlockConfigParseHandler in testContext.MockManagedBlockConfigParseHandlers.Skip(1))
                    mockManagedBlockConfigParseHandler.ShouldNotHaveReceived(x => x.OnParsing(It.IsAny<ManagedBlockConfigLine>()));
            }

            testContext.MockLogger.ShouldNotHaveReceived(x => x.AddLine(It.IsAny<string>()));
        }

        [TestCaseSource(nameof(ValidBlockTagAndConfigDataTestCases))]
        public void BuildParseOperation_OperationIsDisposed_OperationIsRecycled(string blockTag, string[][] configData)
        {
            var testContext = new TestContext()
            {
                BlockTag = blockTag,
                Config = BuildConfig(configData)
            };
            testContext.AddFakeManagedBlockConfigParseHandler(ParseResult.Success);

            var result = testContext.Uut.MakeParseOperation(testContext.MockBlock.Object);
            result.ShouldRunToCompletion(testContext.MockBackgroundWorker);

            var mockConfigParseHandlersInvocations = testContext.MockManagedBlockConfigParseHandlers
                .Select(x => x.Invocations.ToArray())
                .ToArray();
            var mockBlockInvocations = testContext.MockBlock.Invocations
                .ToArray();

            result.ShouldBeAssignableTo<IDisposable>();
            (result as IDisposable).Dispose();

            var secondMockBlock = new Mock<IMyTerminalBlock>();
            secondMockBlock
                .Setup(x => x.CustomName)
                .Returns(() => testContext.MockBlock.Object.CustomName);
            secondMockBlock
                .Setup(x => x.CustomData)
                .Returns(() => testContext.MockBlock.Object.CustomData);

            testContext.MockManagedBlockConfigParseHandlers
                .ForEach(x => x.Invocations.Clear());
            testContext.MockBlock.Invocations.Clear();

            testContext.Uut.MakeParseOperation(secondMockBlock.Object)
                .ShouldBeSameAs(result);

            result.ShouldRunToCompletion(testContext.MockBackgroundWorker);

            foreach (var i in Enumerable.Range(0, testContext.MockManagedBlockConfigParseHandlers.Count))
            {
                testContext.MockManagedBlockConfigParseHandlers[i].Invocations.Count.ShouldBe(mockConfigParseHandlersInvocations[i].Length);
                foreach (var j in Enumerable.Range(0, mockConfigParseHandlersInvocations.Length))
                    testContext.MockManagedBlockConfigParseHandlers[i].Invocations[j].ShouldBe(mockConfigParseHandlersInvocations[i][j]);
            }

            testContext.MockBlock.Invocations.Count.ShouldBe(mockBlockInvocations.Length);
            foreach (var i in Enumerable.Range(0, mockBlockInvocations.Length))
                testContext.MockBlock.Invocations[i].ShouldBe(mockBlockInvocations[i]);

        }

        #endregion MakeParseOperation() Tests
    }

    public static class ManagedBlockConfigLineParseHandlerAssertions
    {
        public static void ShouldHaveReceivedOnParsing(this Mock<IManagedBlockConfigParseHandler> mockHandler, ManagedBlockConfigLine expectedConfigLine)
            => mockHandler
                .ShouldHaveReceived(x => x.OnParsing(It.Is<ManagedBlockConfigLine>(configLine =>
                    (configLine.BlockTag == expectedConfigLine.BlockTag)
                        && (configLine.Option == expectedConfigLine.Option)
                        && (configLine.ParamCount == expectedConfigLine.ParamCount)
                        && Enumerable.Range(0, configLine.ParamCount)
                            .All(index => configLine.GetParam(index) == expectedConfigLine.GetParam(index)))));
    }
}
