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
    public class ConfigManagerTests
    {
        #region Test Context

        public class TestContext
        {
            public TestContext()
            {
                _mockConfigParseHandlers = new List<Mock<IConfigParseHandler>>();
                _configParseHandlers = new List<IConfigParseHandler>();

                MockLogger = new Mock<ILogger>();

                MockProgrammableBlock = new Mock<IMyProgrammableBlock>();
                MockProgrammableBlock
                    .Setup(x => x.CustomData)
                    .Returns(() => Config);

                Uut = new ConfigManager(
                    _configParseHandlers,
                    MockLogger.Object,
                    MockProgrammableBlock.Object);

                MockBackgroundWorker = new FakeBackgroundWorker();
            }

            public IReadOnlyList<Mock<IConfigParseHandler>> MockConfigParseHandlers
                => _mockConfigParseHandlers;

            public readonly Mock<ILogger> MockLogger;

            public readonly Mock<IMyProgrammableBlock> MockProgrammableBlock;

            public readonly ConfigManager Uut;

            public string Config;

            public readonly FakeBackgroundWorker MockBackgroundWorker;

            public Mock<IConfigParseHandler> AddFakeConfigParseHandler(ParseResult parseResult)
            {
                var mockConfigParseHandler = new Mock<IConfigParseHandler>();

                mockConfigParseHandler
                    .Setup(x => x.OnParsing(It.IsAny<ConfigLine>()))
                    .Returns(parseResult);

                _mockConfigParseHandlers.Add(mockConfigParseHandler);
                _configParseHandlers.Add(mockConfigParseHandler.Object);

                return mockConfigParseHandler;
            }

            private List<Mock<IConfigParseHandler>> _mockConfigParseHandlers;

            private List<IConfigParseHandler> _configParseHandlers;
        }

        #endregion Test Context

        #region Test Cases

        public static readonly string[][][] ConfigDatas
            = new[]
            {
                new[] { new[] { "option1"                     }                                           },
                new[] { new[] { "option2", "param1"           }                                           },
                new[] { new[] { "option3", "param2", "param3" }                                           },
                new[] { new[] { "option4"                     }, new[] { "option5" }                      },
                new[] { new[] { "option6"                     }, new[] { "option7" }, new[] { "option8" } },
            };

        public static readonly TestCaseData[] ConfigDataTestCases
            = ConfigDatas
                .Select(configData => new TestCaseData(new object[] { configData })
                    .SetName($"{{m}}(\"{Regex.Escape(BuildConfig(configData))}\")"))
                .ToArray();

        public static string BuildConfig(string[][] configData)
            => string.Join("\n", configData
                .Select(BuildConfigLine));

        public static string BuildConfigLine(string[] configParams)
            => string.Join(":", configParams);

        #endregion Test Cases

        #region MakeParseOperation() Tests

        [TestCaseSource(nameof(ConfigDataTestCases))]
        public void MakeParseOperation_ConfigParseHandlersIsEmpty_CompletesImmediately(string[][] configData)
        {
            var testContext = new TestContext()
            {
                Config = BuildConfig(configData)
            };

            testContext.Uut.MakeParseOperation()
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
            testContext.AddFakeConfigParseHandler(ParseResult.Ignored);

            testContext.Uut.MakeParseOperation()
                .ShouldRunToCompletion(testContext.MockBackgroundWorker);

            foreach (var mockConfigParseHandler in testContext.MockConfigParseHandlers)
            {
                mockConfigParseHandler.ShouldHaveReceived(x => x.OnStarting(), 1);
                mockConfigParseHandler.ShouldNotHaveReceived(x => x.OnParsing(It.IsAny<ConfigLine>()));
                mockConfigParseHandler.ShouldHaveReceived(x => x.OnCompleted(), 1);
            }

            testContext.MockLogger.ShouldNotHaveReceived(x => x.AddLine(It.IsAny<string>()));
        }

        [TestCaseSource(nameof(ConfigDataTestCases))]
        public void MakeParseOperation_Always_SplitsLinesAndParamsCorrectly(string[][] configData)
        {
            var testContext = new TestContext()
            {
                Config = BuildConfig(configData)
            };
            var mockConfigParseHandler = testContext.AddFakeConfigParseHandler(ParseResult.Success);

            var configLines = new List<ConfigLine>();

            mockConfigParseHandler
                .Setup(x => x.OnParsing(It.IsAny<ConfigLine>()))
                .Callback<ConfigLine>(x => configLines.Add(x))
                .Returns(ParseResult.Success);

            testContext.Uut.MakeParseOperation()
                .ShouldRunToCompletion(testContext.MockBackgroundWorker);

            configLines.Count.ShouldBe(configData.Length);
            foreach(var i in Enumerable.Range(0, configLines.Count))
            {
                var expectedConfigLine = new ConfigLine(configData[i]);

                configLines[i].Option.ShouldBe(expectedConfigLine.Option);
                configLines[i].ParamCount.ShouldBe(expectedConfigLine.ParamCount);

                foreach (var index in Enumerable.Range(0, configLines[i].ParamCount))
                    configLines[i].GetParam(index).ShouldBe(expectedConfigLine.GetParam(index));
            }
        }

        [TestCaseSource(nameof(ConfigDataTestCases))]
        public void MakeParseOperation_LineIsIgnored_LogsError(string[][] configData)
        {
            var testContext = new TestContext()
            {
                Config = BuildConfig(configData)
            };
            foreach (var _ in Enumerable.Repeat(0, 3))
                testContext.AddFakeConfigParseHandler(ParseResult.Ignored);

            testContext.Uut.MakeParseOperation()
                .ShouldRunToCompletion(testContext.MockBackgroundWorker);

            foreach (var configLineData in configData)
            {
                foreach (var mockConfigParseHandler in testContext.MockConfigParseHandlers)
                {
                    mockConfigParseHandler.ShouldHaveReceived(x => x.OnStarting(), 1);
                    mockConfigParseHandler.ShouldHaveReceivedOnParsing(new ConfigLine(configLineData));
                    mockConfigParseHandler.ShouldHaveReceived(x => x.OnCompleted(), 1);
                }

                testContext.MockLogger.ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.Contains(BuildConfigLine(configLineData)))));
            }
        }

        [TestCaseSource(nameof(ConfigDataTestCases))]
        public void MakeParseOperation_LineParseIsError_LogsErrorAndEndsLineParse(string[][] configData)
        {
            var testContext = new TestContext()
            {
                Config = BuildConfig(configData)
            };

            var errorText = "errorText";
            foreach (var handlerIndex in Enumerable.Repeat(0, 3))
                testContext.AddFakeConfigParseHandler(ParseResult.FromError($"{errorText}{handlerIndex}"));

            testContext.Uut.MakeParseOperation()
                .ShouldRunToCompletion(testContext.MockBackgroundWorker);

            foreach (var configLineData in configData)
            {
                foreach (var mockConfigParseHandler in testContext.MockConfigParseHandlers)
                {
                    mockConfigParseHandler.ShouldHaveReceived(x => x.OnStarting(), 1);
                    mockConfigParseHandler.ShouldHaveReceived(x => x.OnCompleted(), 1);
                }

                testContext.MockConfigParseHandlers[0]
                    .ShouldHaveReceivedOnParsing(new ConfigLine(configLineData));

                foreach (var mockConfigParseHandler in testContext.MockConfigParseHandlers.Skip(1))
                    mockConfigParseHandler.ShouldNotHaveReceived(x => x.OnParsing(It.IsAny<ConfigLine>()));

                testContext.MockLogger.ShouldHaveReceived(x => x.AddLine(It.Is<string>(y =>
                    y.Contains(BuildConfigLine(configLineData))
                        && y.Contains($"{errorText}0"))));

                foreach(var i in Enumerable.Range(0, testContext.MockConfigParseHandlers.Count).Skip(1))
                    testContext.MockLogger.ShouldNotHaveReceived(x => x.AddLine(It.Is<string>(y =>
                        y.Contains(BuildConfigLine(configLineData))
                            && y.Contains($"{errorText}{i}"))));
            }
        }

        [TestCaseSource(nameof(ConfigDataTestCases))]
        public void MakeParseOperation_LineParseIsSuccessful_EndsLineParse(string[][] configData)
        {
            var testContext = new TestContext()
            {
                Config = BuildConfig(configData)
            };

            foreach (var handlerIndex in Enumerable.Repeat(0, 3))
                testContext.AddFakeConfigParseHandler(ParseResult.Success);

            testContext.Uut.MakeParseOperation()
                .ShouldRunToCompletion(testContext.MockBackgroundWorker);

            foreach (var configLineData in configData)
            {
                foreach (var mockConfigParseHandler in testContext.MockConfigParseHandlers)
                {
                    mockConfigParseHandler.ShouldHaveReceived(x => x.OnStarting(), 1);
                    mockConfigParseHandler.ShouldHaveReceived(x => x.OnCompleted(), 1);
                }

                testContext.MockConfigParseHandlers[0]
                    .ShouldHaveReceivedOnParsing(new ConfigLine(configLineData));

                foreach (var mockConfigParseHandler in testContext.MockConfigParseHandlers.Skip(1))
                    mockConfigParseHandler.ShouldNotHaveReceived(x => x.OnParsing(It.IsAny<ConfigLine>()));
            }

            testContext.MockLogger.ShouldNotHaveReceived(x => x.AddLine(It.IsAny<string>()));
        }

        [TestCaseSource(nameof(ConfigDataTestCases))]
        public void MakeParseOperation_OperationIsDisposed_OperationIsRecycled(string[][] configData)
        {
            var testContext = new TestContext()
            {
                Config = BuildConfig(configData)
            };
            testContext.AddFakeConfigParseHandler(ParseResult.Success);

            var result = testContext.Uut.MakeParseOperation();
            result.ShouldRunToCompletion(testContext.MockBackgroundWorker);

            var mockConfigParseHandlersInvocations = testContext.MockConfigParseHandlers
                .Select(x => x.Invocations.ToArray())
                .ToArray();

            result.ShouldBeAssignableTo<IDisposable>();
            (result as IDisposable).Dispose();

            testContext.MockConfigParseHandlers
                .ForEach(x => x.Invocations.Clear());

            testContext.Uut.MakeParseOperation()
                .ShouldBeSameAs(result);

            result.ShouldRunToCompletion(testContext.MockBackgroundWorker);

            foreach (var i in Enumerable.Range(0, testContext.MockConfigParseHandlers.Count))
            {
                testContext.MockConfigParseHandlers[i].Invocations.Count.ShouldBe(mockConfigParseHandlersInvocations[i].Length);
                foreach (var j in Enumerable.Range(0, mockConfigParseHandlersInvocations.Length))
                    testContext.MockConfigParseHandlers[i].Invocations[j].ShouldBe(mockConfigParseHandlersInvocations[i][j]);
            }
        }

        #endregion MakeParseOperation() Tests
    }

    public static class ConfigParseHandlerAssertions
    {
        public static void ShouldHaveReceivedOnParsing(this Mock<IConfigParseHandler> mockHandler, ConfigLine expectedConfigLine)
            => mockHandler
                .ShouldHaveReceived(x => x.OnParsing(It.Is<ConfigLine>(configLine =>
                    (configLine.Option == expectedConfigLine.Option)
                        && (configLine.ParamCount == expectedConfigLine.ParamCount)
                        && Enumerable.Range(0, configLine.ParamCount)
                            .All(index => configLine.GetParam(index) == expectedConfigLine.GetParam(index)))));
    }
}
