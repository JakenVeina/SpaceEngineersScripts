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
    public class GasTankCollectionHandlerTests
    {
        #region Test Context

        private class TestContext
        {
            public int GasTankCount;

            public bool IgnoreGasTanks
                = false;

            public bool IgnoreOtherBlocks
                = false;

            public TestContext()
            {
                MockGasTankManager = new Mock<IGasTankManager>();
                MockGasTankManager
                    .Setup(x => x.GasTanks.Count)
                    .Returns(() => GasTankCount);

                MockLogger = new Mock<ILogger>();

                MockDockingManagerSettingsProvider = new Mock<IDockingManagerSettingsProvider>();
                MockDockingManagerSettingsProvider
                    .Setup(x => x.Settings)
                    .Returns(() => new DockingManagerSettings()
                    {
                        IgnoreBatteryBlocks = IgnoreOtherBlocks,
                        IgnoreBeacons = IgnoreOtherBlocks,
                        IgnoreGasGenerators = IgnoreOtherBlocks,
                        IgnoreGasTanks = IgnoreGasTanks,
                        IgnoreGyros = IgnoreOtherBlocks,
                        IgnoreLandingGears = IgnoreOtherBlocks,
                        IgnoreLightingBlocks = IgnoreOtherBlocks,
                        IgnoreRadioAntennae = IgnoreOtherBlocks,
                        IgnoreReactors = IgnoreOtherBlocks
                    });

                Uut = new GasTankCollectionHandler(
                    MockGasTankManager.Object,
                    MockLogger.Object,
                    MockDockingManagerSettingsProvider.Object);

                MockBackgroundWorker = new FakeBackgroundWorker();
            }

            public readonly Mock<IGasTankManager> MockGasTankManager;

            public readonly Mock<ILogger> MockLogger;

            public readonly Mock<IDockingManagerSettingsProvider> MockDockingManagerSettingsProvider;

            public readonly GasTankCollectionHandler Uut;

            public readonly FakeBackgroundWorker MockBackgroundWorker;
        }

        #endregion Test Context

        #region OnStarting() Tests

        [Test]
        public void OnStarting_Always_ClearsGasTanks()
        {
            var testContext = new TestContext();

            testContext.Uut.OnStarting();

            testContext.MockGasTankManager
                .ShouldHaveReceived(x => x.ClearGasTanks());
        }

        #endregion OnStarting() Tests

        #region MakeCollectBlockOperation Tests

        [Test]
        public void MakeCollectBlockOperation_BlockIsNotGasTank_IgnoresBlock()
        {
            var testContext = new TestContext();

            var mockBlock = new Mock<IMyTerminalBlock>();

            var result = testContext.Uut.MakeCollectBlockOperation(mockBlock.Object);
            result.ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            testContext.MockBackgroundWorker.MockSubOperationScheduler
                .ShouldNotHaveReceived(x => x(It.IsAny<IBackgroundOperation>()));

            testContext.MockGasTankManager
                .ShouldNotHaveReceived(x => x.AddGasTank(It.IsAny<IMyGasTank>()));

            result.Result.IsIgnored.ShouldBeTrue();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void MakeCollectBlockOperation_BlockIsGasTank_AddsGasTank(bool ignoreOtherBlocks)
        {
            var testContext = new TestContext()
            {
                IgnoreOtherBlocks = ignoreOtherBlocks
            };

            var mockBlock = new Mock<IMyGasTank>();

            var result = testContext.Uut.MakeCollectBlockOperation(mockBlock.Object);
            result.ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            testContext.MockBackgroundWorker.MockSubOperationScheduler
                .ShouldNotHaveReceived(x => x(It.IsAny<IBackgroundOperation>()));

            testContext.MockGasTankManager
                .ShouldHaveReceived(x => x.AddGasTank(mockBlock.Object));

            result.Result.IsSuccess.ShouldBeTrue();
        }

        [Test]
        public void MakeCollectBlockOperation_GasTanksAreIgnored_IgnoresBlock()
        {
            var testContext = new TestContext()
            {
                IgnoreGasTanks = true
            };

            var mockBlock = new Mock<IMyTerminalBlock>();

            var result = testContext.Uut.MakeCollectBlockOperation(mockBlock.Object);
            result.ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            testContext.MockBackgroundWorker.MockSubOperationScheduler
                .ShouldNotHaveReceived(x => x(It.IsAny<IBackgroundOperation>()));

            testContext.MockGasTankManager
                .ShouldNotHaveReceived(x => x.AddGasTank(It.IsAny<IMyGasTank>()));

            result.Result.IsIgnored.ShouldBeTrue();
        }

        [Test]
        public void MakeCollectBlockOperation_OperationIsDisposed_RecyclesOperation()
        {
            var testContext = new TestContext();

            var mockBlock = new Mock<IMyGasTank>();

            var result = testContext.Uut.MakeCollectBlockOperation(mockBlock.Object);
            result.ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            var mockBlockInvocations = mockBlock
                .Invocations.ToArray();

            var mockDoorManagerInvocations = testContext.MockGasTankManager
                .Invocations.ToArray();

            result.ShouldBeAssignableTo<IDisposable>();
            (result as IDisposable).Dispose();

            mockBlock
                .Invocations.Clear();

            testContext.MockGasTankManager
                .Invocations.Clear();

            var secondMockBlock = new Mock<IMyGasTank>();

            testContext.Uut.MakeCollectBlockOperation(secondMockBlock.Object)
                .ShouldBeSameAs(result);

            result.ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            secondMockBlock.Invocations.Count.ShouldBe(mockBlockInvocations.Length);
            foreach (var i in Enumerable.Range(0, mockBlockInvocations.Length))
                secondMockBlock.Invocations[i].ShouldBe(mockBlockInvocations[i]);

            testContext.MockGasTankManager.Invocations.Count.ShouldBe(mockDoorManagerInvocations.Length);
            foreach (var i in Enumerable.Range(0, mockDoorManagerInvocations.Length))
                testContext.MockGasTankManager.Invocations[i].ShouldBe(mockDoorManagerInvocations[i]);
        }

        #endregion MakeCollectBlockOperation Tests

        #region OnCompleted() Tests

        [Test]
        public void OnCompleted_GasTankCountIs0_DoesNotLogGasTankCount()
        {
            var testContext = new TestContext()
            {
                GasTankCount = 0
            };

            testContext.Uut.OnCompleted();

            testContext.MockLogger
                .ShouldNotHaveReceived(x => x.AddLine(It.IsAny<string>()));
        }

        [TestCase(1)]
        [TestCase(10)]
        public void OnCompleted_GasTankCountIsGreaterThan0_LogsGasTankCount(int gasTankCount)
        {
            var testContext = new TestContext()
            {
                GasTankCount = gasTankCount
            };

            testContext.Uut.OnCompleted();

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.Contains(gasTankCount.ToString()))));
        }

        #endregion OnCompleted() Tests
    }
}
