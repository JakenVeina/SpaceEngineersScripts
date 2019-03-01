using System;
using System.Linq;

using Moq;
using NUnit.Framework;
using Shouldly;

using Sandbox.ModAPI.Ingame;

using static IngameScript.Program;

using Mixins.Test.Common;

namespace AutomatedDockingProcedures.Test
{
    [TestFixture]
    public class FunctionalBlockCollectionHandlerTests
    {
        #region Test Context

        private class TestContext
        {
            public int FunctionalBlockCount;

            public bool IgnoreBeacons
                = false;

            public bool IgnoreGasGenerators
                = false;

            public bool IgnoreGyros
                = false;

            public bool IgnoreLightingBlocks
                = false;

            public bool IgnoreRadioAntennae
                = false;

            public bool IgnoreReactors
                = false;

            public bool IgnoreOtherBlocks
                = false;

            public TestContext()
            {
                MockFunctionalBlockManager = new Mock<IFunctionalBlockManager>();
                MockFunctionalBlockManager
                    .Setup(x => x.FunctionalBlocks.Count)
                    .Returns(() => FunctionalBlockCount);

                MockLogger = new Mock<ILogger>();

                MockProgramSettingsProvider = new Mock<IProgramSettingsProvider>();
                MockProgramSettingsProvider
                    .Setup(x => x.Settings)
                    .Returns(() => new ProgramSettings()
                    {
                        IgnoreBatteryBlocks = IgnoreOtherBlocks,
                        IgnoreBeacons = IgnoreBeacons,
                        IgnoreGasGenerators = IgnoreGasGenerators,
                        IgnoreGasTanks = IgnoreOtherBlocks,
                        IgnoreGyros = IgnoreGyros,
                        IgnoreLandingGears = IgnoreOtherBlocks,
                        IgnoreLightingBlocks = IgnoreLightingBlocks,
                        IgnoreRadioAntennae = IgnoreRadioAntennae,
                        IgnoreReactors = IgnoreReactors
                    });

                Uut = new FunctionalBlockCollectionHandler(
                    MockFunctionalBlockManager.Object,
                    MockLogger.Object,
                    MockProgramSettingsProvider.Object);

                MockBackgroundWorker = new FakeBackgroundWorker();
            }

            public readonly Mock<IFunctionalBlockManager> MockFunctionalBlockManager;

            public readonly Mock<ILogger> MockLogger;

            public readonly Mock<IProgramSettingsProvider> MockProgramSettingsProvider;

            public readonly FunctionalBlockCollectionHandler Uut;

            public readonly FakeBackgroundWorker MockBackgroundWorker;

            public Moq.Mock MakeFakeBlock(Type blockType)
                => Activator.CreateInstance(typeof(Mock<>).MakeGenericType(blockType)) as Moq.Mock;
        }

        #endregion Test Context

        #region OnStarting() Tests

        [Test]
        public void OnStarting_Always_ClearsFunctionalBlocks()
        {
            var testContext = new TestContext();

            testContext.Uut.OnStarting();

            testContext.MockFunctionalBlockManager
                .ShouldHaveReceived(x => x.ClearFunctionalBlocks());
        }

        #endregion OnStarting() Tests

        #region MakeCollectBlockOperation Tests

        [TestCase(typeof(IMyTerminalBlock))]
        [TestCase(typeof(IMyFunctionalBlock))]
        [TestCase(typeof(IMyDoor))]
        public void MakeCollectBlockOperation_BlockIsNotRelevantType_IgnoresBlock(Type blockType)
        {
            var testContext = new TestContext();

            var mockBlock = testContext.MakeFakeBlock(blockType);

            var result = testContext.Uut.MakeCollectBlockOperation(mockBlock.Object as IMyFunctionalBlock);
            result.ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            testContext.MockBackgroundWorker.MockSubOperationScheduler
                .ShouldNotHaveReceived(x => x(It.IsAny<IBackgroundOperation>()));

            testContext.MockFunctionalBlockManager
                .ShouldNotHaveReceived(x => x.AddFunctionalBlock(It.IsAny<IMyFunctionalBlock>()));

            result.Result.IsIgnored.ShouldBeTrue();
        }

        [TestCase(typeof(IMyBeacon),        false, true,  true,  true,  true,  true,  true )]
        [TestCase(typeof(IMyBeacon),        false, false, false, false, false, false, false)]
        [TestCase(typeof(IMyGasGenerator),  true,  false, true,  true,  true,  true,  true )]
        [TestCase(typeof(IMyGasGenerator),  false, false, false, false, false, false, false)]
        [TestCase(typeof(IMyGyro),          true,  true,  false, true,  true,  true,  true )]
        [TestCase(typeof(IMyGyro),          false, false, false, false, false, false, false)]
        [TestCase(typeof(IMyLightingBlock), true,  true,  true,  false, true,  true,  true )]
        [TestCase(typeof(IMyLightingBlock), false, false, false, false, false, false, false)]
        [TestCase(typeof(IMyRadioAntenna),  true,  true,  true,  true,  false, true,  true )]
        [TestCase(typeof(IMyRadioAntenna),  false, false, false, false, false, false, false)]
        [TestCase(typeof(IMyReactor),       true,  true,  true,  true,  true,  false, true )]
        [TestCase(typeof(IMyReactor),       false, false, false, false, false, false, false)]
        public void MakeCollectBlockOperation_BlockIsRelevantFunctionalBlock_AddsFunctionalBlock(
            Type blockType,
            bool ignoreBeacons,
            bool ignoreGasGenerators,
            bool ignoreGyros,
            bool ignoreLightingBlocks,
            bool ignoreRadioAntennae,
            bool ignoreReactors,
            bool ignoreOtherBlocks)
        {
            var testContext = new TestContext()
            {
                IgnoreBeacons = ignoreBeacons,
                IgnoreGasGenerators = ignoreGasGenerators,
                IgnoreGyros = ignoreGyros,
                IgnoreLightingBlocks = ignoreLightingBlocks,
                IgnoreRadioAntennae = ignoreRadioAntennae,
                IgnoreReactors = ignoreReactors,
                IgnoreOtherBlocks = ignoreOtherBlocks
            };

            var mockBlock = testContext.MakeFakeBlock(blockType);

            var result = testContext.Uut.MakeCollectBlockOperation(mockBlock.Object as IMyFunctionalBlock);
            result.ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            testContext.MockBackgroundWorker.MockSubOperationScheduler
                .ShouldNotHaveReceived(x => x(It.IsAny<IBackgroundOperation>()));

            testContext.MockFunctionalBlockManager
                .ShouldHaveReceived(x => x.AddFunctionalBlock(mockBlock.Object as IMyFunctionalBlock));

            result.Result.IsSuccess.ShouldBeTrue();
        }

        [TestCase(typeof(IMyBeacon),        true,  false, false, false, false, false)]
        [TestCase(typeof(IMyGasGenerator),  false, true,  false, false, false, false)]
        [TestCase(typeof(IMyGyro),          false, false, true,  false, false, false)]
        [TestCase(typeof(IMyLightingBlock), false, false, false, true,  false, false)]
        [TestCase(typeof(IMyRadioAntenna),  false, false, false, false, true,  false)]
        [TestCase(typeof(IMyReactor),       false, false, false, false, false, true )]
        public void MakeCollectBlockOperation_BlockTypeIsIgnored_IgnoresBlock(
            Type blockType,
            bool ignoreBeacons,
            bool ignoreGasGenerators,
            bool ignoreGyros,
            bool ignoreLightingBlocks,
            bool ignoreRadioAntennae,
            bool ignoreReactors)
        {
            var testContext = new TestContext()
            {
                IgnoreBeacons = ignoreBeacons,
                IgnoreGasGenerators = ignoreGasGenerators,
                IgnoreGyros = ignoreGyros,
                IgnoreLightingBlocks = ignoreLightingBlocks,
                IgnoreRadioAntennae = ignoreRadioAntennae,
                IgnoreReactors = ignoreReactors
            };

            var mockBlock = testContext.MakeFakeBlock(blockType);

            var result = testContext.Uut.MakeCollectBlockOperation(mockBlock.Object as IMyFunctionalBlock);
            result.ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            testContext.MockBackgroundWorker.MockSubOperationScheduler
                .ShouldNotHaveReceived(x => x(It.IsAny<IBackgroundOperation>()));

            testContext.MockFunctionalBlockManager
                .ShouldNotHaveReceived(x => x.AddFunctionalBlock(It.IsAny<IMyFunctionalBlock>()));

            result.Result.IsIgnored.ShouldBeTrue();
        }

        [TestCase(typeof(IMyGyro))]
        [TestCase(typeof(IMyLightingBlock))]
        [TestCase(typeof(IMyBeacon))]
        [TestCase(typeof(IMyRadioAntenna))]
        [TestCase(typeof(IMyGasGenerator))]
        [TestCase(typeof(IMyReactor))]
        public void MakeCollectBlockOperation_OperationIsDisposed_RecyclesOperation(Type blockType)
        {
            var testContext = new TestContext();

            var mockBlock = testContext.MakeFakeBlock(blockType);

            var result = testContext.Uut.MakeCollectBlockOperation(mockBlock.Object as IMyFunctionalBlock);
            result.ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            var mockBlockInvocations = mockBlock
                .Invocations.ToArray();

            var mockDoorManagerInvocations = testContext.MockFunctionalBlockManager
                .Invocations.ToArray();

            result.ShouldBeAssignableTo<IDisposable>();
            (result as IDisposable).Dispose();

            mockBlock
                .Invocations.Clear();

            testContext.MockFunctionalBlockManager
                .Invocations.Clear();

            var secondMockBlock = testContext.MakeFakeBlock(blockType);

            testContext.Uut.MakeCollectBlockOperation(secondMockBlock.Object as IMyFunctionalBlock)
                .ShouldBeSameAs(result);

            result.ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            secondMockBlock.Invocations.Count.ShouldBe(mockBlockInvocations.Length);
            foreach (var i in Enumerable.Range(0, mockBlockInvocations.Length))
                secondMockBlock.Invocations[i].ShouldBe(mockBlockInvocations[i]);

            testContext.MockFunctionalBlockManager.Invocations.Count.ShouldBe(mockDoorManagerInvocations.Length);
            foreach (var i in Enumerable.Range(0, mockDoorManagerInvocations.Length))
                testContext.MockFunctionalBlockManager.Invocations[i].ShouldBe(mockDoorManagerInvocations[i]);
        }

        #endregion MakeCollectBlockOperation Tests

        #region OnCompleted() Tests

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void OnCompleted_Always_LogsFunctionalBlockCount(int functionalBlockCount)
        {
            var testContext = new TestContext()
            {
                FunctionalBlockCount = functionalBlockCount
            };

            testContext.Uut.OnCompleted();

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.Contains(functionalBlockCount.ToString()))));
        }

        #endregion OnCompleted() Tests
    }
}
