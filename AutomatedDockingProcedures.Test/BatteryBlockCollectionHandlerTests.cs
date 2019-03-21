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
    public class BatteryBlockCollectionHandlerTests
    {
        #region Test Context

        private class TestContext
        {
            public int BatteryBlockCount;

            public bool IgnoreBatteryBlocks
                = false;

            public bool IgnoreOtherBlocks
                = false;

            public TestContext()
            {
                MockBatteryBlockManager = new Mock<IBatteryBlockManager>();
                MockBatteryBlockManager
                    .Setup(x => x.BatteryBlocks.Count)
                    .Returns(() => BatteryBlockCount);

                MockLogger = new Mock<ILogger>();

                MockDockingManagerSettingsProvider = new Mock<IDockingManagerSettingsProvider>();
                MockDockingManagerSettingsProvider
                    .Setup(x => x.Settings)
                    .Returns(() => new DockingManagerSettings()
                    {
                        IgnoreBatteryBlocks = IgnoreBatteryBlocks,
                        IgnoreBeacons = IgnoreOtherBlocks,
                        IgnoreGasGenerators = IgnoreOtherBlocks,
                        IgnoreGasTanks = IgnoreOtherBlocks,
                        IgnoreGyros = IgnoreOtherBlocks,
                        IgnoreLandingGears = IgnoreOtherBlocks,
                        IgnoreLightingBlocks = IgnoreOtherBlocks,
                        IgnoreRadioAntennae = IgnoreOtherBlocks,
                        IgnoreReactors = IgnoreOtherBlocks
                    });

                Uut = new BatteryBlockCollectionHandler(
                    MockBatteryBlockManager.Object,
                    MockLogger.Object,
                    MockDockingManagerSettingsProvider.Object);

                MockBackgroundWorker = new FakeBackgroundWorker();
            }

            public readonly Mock<IBatteryBlockManager> MockBatteryBlockManager;

            public readonly Mock<ILogger> MockLogger;

            public readonly Mock<IDockingManagerSettingsProvider> MockDockingManagerSettingsProvider;

            public readonly BatteryBlockCollectionHandler Uut;

            public readonly FakeBackgroundWorker MockBackgroundWorker;
        }

        #endregion Test Context

        #region OnStarting() Tests

        [Test]
        public void OnStarting_Always_ClearsBatteryBlocks()
        {
            var testContext = new TestContext();

            testContext.Uut.OnStarting();

            testContext.MockBatteryBlockManager
                .ShouldHaveReceived(x => x.ClearBatteryBlocks());
        }

        #endregion OnStarting() Tests

        #region MakeCollectBlockOperation Tests

        [Test]
        public void MakeCollectBlockOperation_BlockIsNotBatteryBlock_IgnoresBlock()
        {
            var testContext = new TestContext();

            var mockBlock = new Mock<IMyTerminalBlock>();

            var result = testContext.Uut.MakeCollectBlockOperation(mockBlock.Object);
            result.ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            testContext.MockBackgroundWorker.MockSubOperationScheduler
                .ShouldNotHaveReceived(x => x(It.IsAny<IBackgroundOperation>()));

            testContext.MockBatteryBlockManager
                .ShouldNotHaveReceived(x => x.AddBatteryBlock(It.IsAny<IMyBatteryBlock>()));

            result.Result.IsIgnored.ShouldBeTrue();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void MakeCollectBlockOperation_BlockIsBatteryBlock_AddsBatteryBlock(bool ignoreOtherBlocks)
        {
            var testContext = new TestContext()
            {
                IgnoreOtherBlocks = ignoreOtherBlocks
            };

            var mockBlock = new Mock<IMyBatteryBlock>();

            var result = testContext.Uut.MakeCollectBlockOperation(mockBlock.Object);
            result.ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            testContext.MockBackgroundWorker.MockSubOperationScheduler
                .ShouldNotHaveReceived(x => x(It.IsAny<IBackgroundOperation>()));

            testContext.MockBatteryBlockManager
                .ShouldHaveReceived(x => x.AddBatteryBlock(mockBlock.Object));

            result.Result.IsSuccess.ShouldBeTrue();
        }

        [Test]
        public void MakeCollectBlockOperation_BatteryBlocksAreIgnored_IgnoresBlock()
        {
            var testContext = new TestContext()
            {
                IgnoreBatteryBlocks = true
            };

            var mockBlock = new Mock<IMyBatteryBlock>();

            var result = testContext.Uut.MakeCollectBlockOperation(mockBlock.Object);
            result.ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            testContext.MockBackgroundWorker.MockSubOperationScheduler
                .ShouldNotHaveReceived(x => x(It.IsAny<IBackgroundOperation>()));

            testContext.MockBatteryBlockManager
                .ShouldNotHaveReceived(x => x.AddBatteryBlock(It.IsAny<IMyBatteryBlock>()));

            result.Result.IsIgnored.ShouldBeTrue();
        }

        [Test]
        public void MakeCollectBlockOperation_OperationIsDisposed_RecyclesOperation()
        {
            var testContext = new TestContext();

            var mockBlock = new Mock<IMyBatteryBlock>();

            var result = testContext.Uut.MakeCollectBlockOperation(mockBlock.Object);
            result.ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            var mockBlockInvocations = mockBlock
                .Invocations.ToArray();

            var mockDoorManagerInvocations = testContext.MockBatteryBlockManager
                .Invocations.ToArray();

            result.ShouldBeAssignableTo<IDisposable>();
            (result as IDisposable).Dispose();

            mockBlock
                .Invocations.Clear();

            testContext.MockBatteryBlockManager
                .Invocations.Clear();

            var secondMockBlock = new Mock<IMyBatteryBlock>();

            testContext.Uut.MakeCollectBlockOperation(secondMockBlock.Object)
                .ShouldBeSameAs(result);

            result.ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            secondMockBlock.Invocations.Count.ShouldBe(mockBlockInvocations.Length);
            foreach (var i in Enumerable.Range(0, mockBlockInvocations.Length))
                secondMockBlock.Invocations[i].ShouldBe(mockBlockInvocations[i]);

            testContext.MockBatteryBlockManager.Invocations.Count.ShouldBe(mockDoorManagerInvocations.Length);
            foreach (var i in Enumerable.Range(0, mockDoorManagerInvocations.Length))
                testContext.MockBatteryBlockManager.Invocations[i].ShouldBe(mockDoorManagerInvocations[i]);
        }

        #endregion MakeCollectBlockOperation Tests

        #region OnCompleted() Tests

        [Test]
        public void OnCompleted_BatteryBlockCountIs0_DoesNotLogBatteryBlockCount()
        {
            var testContext = new TestContext()
            {
                BatteryBlockCount = 0
            };

            testContext.Uut.OnCompleted();

            testContext.MockLogger
                .ShouldNotHaveReceived(x => x.AddLine(It.IsAny<string>()));
        }

        [TestCase(1)]
        [TestCase(10)]
        public void OnCompleted_BatteryBlockCountIsGreaterThan0_LogsBatteryBlockCount(int batteryBlockCount)
        {
            var testContext = new TestContext()
            {
                BatteryBlockCount = batteryBlockCount
            };

            testContext.Uut.OnCompleted();

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.Contains(batteryBlockCount.ToString()))));
        }

        #endregion OnCompleted() Tests
    }
}
