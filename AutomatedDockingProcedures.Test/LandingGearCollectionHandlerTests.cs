using System;
using System.Linq;

using Moq;
using NUnit.Framework;
using Shouldly;

using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;

using static IngameScript.Program;

using Mixins.Test.Common;

namespace AutomatedDockingProcedures.Test
{
    [TestFixture]
    public class LandingGearCollectionHandlerTests
    {
        #region Test Context

        private class TestContext
        {
            public int LandingGearCount;

            public bool IgnoreLandingGears
                = false;

            public bool IgnoreOtherBlocks
                = false;

            public TestContext()
            {
                MockLandingGearManager = new Mock<ILandingGearManager>();
                MockLandingGearManager
                    .Setup(x => x.LandingGears.Count)
                    .Returns(() => LandingGearCount);

                MockLogger = new Mock<ILogger>();

                MockDockingManagerSettingsProvider = new Mock<IDockingManagerSettingsProvider>();
                MockDockingManagerSettingsProvider
                    .Setup(x => x.Settings)
                    .Returns(() => new DockingManagerSettings()
                    {
                        IgnoreBatteryBlocks = IgnoreOtherBlocks,
                        IgnoreBeacons = IgnoreOtherBlocks,
                        IgnoreGasGenerators = IgnoreOtherBlocks,
                        IgnoreGasTanks = IgnoreOtherBlocks,
                        IgnoreGyros = IgnoreOtherBlocks,
                        IgnoreLandingGears = IgnoreLandingGears,
                        IgnoreLightingBlocks = IgnoreOtherBlocks,
                        IgnoreRadioAntennae = IgnoreOtherBlocks,
                        IgnoreReactors = IgnoreOtherBlocks
                    });

                Uut = new LandingGearCollectionHandler(
                    MockLandingGearManager.Object,
                    MockLogger.Object,
                    MockDockingManagerSettingsProvider.Object);

                MockBackgroundWorker = new FakeBackgroundWorker();
            }

            public readonly Mock<ILandingGearManager> MockLandingGearManager;

            public readonly Mock<ILogger> MockLogger;

            public readonly Mock<IDockingManagerSettingsProvider> MockDockingManagerSettingsProvider;

            public readonly LandingGearCollectionHandler Uut;

            public readonly FakeBackgroundWorker MockBackgroundWorker;
        }

        #endregion Test Context

        #region OnStarting() Tests

        [Test]
        public void OnStarting_Always_ClearsLandingGears()
        {
            var testContext = new TestContext();

            testContext.Uut.OnStarting();

            testContext.MockLandingGearManager
                .ShouldHaveReceived(x => x.ClearLandingGears());
        }

        #endregion OnStarting() Tests

        #region MakeCollectBlockOperation Tests

        [Test]
        public void MakeCollectBlockOperation_BlockIsNotLandingGear_IgnoresBlock()
        {
            var testContext = new TestContext();

            var mockBlock = new Mock<IMyTerminalBlock>();

            var result = testContext.Uut.MakeCollectBlockOperation(mockBlock.Object);
            result.ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            testContext.MockBackgroundWorker.MockSubOperationScheduler
                .ShouldNotHaveReceived(x => x(It.IsAny<IBackgroundOperation>()));

            testContext.MockLandingGearManager
                .ShouldNotHaveReceived(x => x.AddLandingGear(It.IsAny<IMyLandingGear>()));

            result.Result.IsIgnored.ShouldBeTrue();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void MakeCollectBlockOperation_BlockIsLandingGear_AddsLandingGear(bool ignoreOtherBlocks)
        {
            var testContext = new TestContext()
            {
                IgnoreOtherBlocks = ignoreOtherBlocks
            };

            var mockBlock = new Mock<IMyLandingGear>();

            var result = testContext.Uut.MakeCollectBlockOperation(mockBlock.Object);
            result.ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            testContext.MockBackgroundWorker.MockSubOperationScheduler
                .ShouldNotHaveReceived(x => x(It.IsAny<IBackgroundOperation>()));

            testContext.MockLandingGearManager
                .ShouldHaveReceived(x => x.AddLandingGear(mockBlock.Object));

            result.Result.IsSuccess.ShouldBeTrue();
        }

        [Test]
        public void MakeCollectBlockOperation_LandingGearsAreIgnored_IgnoresBlock()
        {
            var testContext = new TestContext()
            {
                IgnoreLandingGears = true
            };

            var mockBlock = new Mock<IMyTerminalBlock>();

            var result = testContext.Uut.MakeCollectBlockOperation(mockBlock.Object);
            result.ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            testContext.MockBackgroundWorker.MockSubOperationScheduler
                .ShouldNotHaveReceived(x => x(It.IsAny<IBackgroundOperation>()));

            testContext.MockLandingGearManager
                .ShouldNotHaveReceived(x => x.AddLandingGear(It.IsAny<IMyLandingGear>()));

            result.Result.IsIgnored.ShouldBeTrue();
        }

        [Test]
        public void MakeCollectBlockOperation_OperationIsDisposed_RecyclesOperation()
        {
            var testContext = new TestContext();

            var mockBlock = new Mock<IMyLandingGear>();

            var result = testContext.Uut.MakeCollectBlockOperation(mockBlock.Object);
            result.ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            var mockBlockInvocations = mockBlock
                .Invocations.ToArray();

            var mockDoorManagerInvocations = testContext.MockLandingGearManager
                .Invocations.ToArray();

            result.ShouldBeAssignableTo<IDisposable>();
            (result as IDisposable).Dispose();

            mockBlock
                .Invocations.Clear();

            testContext.MockLandingGearManager
                .Invocations.Clear();

            var secondMockBlock = new Mock<IMyLandingGear>();

            testContext.Uut.MakeCollectBlockOperation(secondMockBlock.Object)
                .ShouldBeSameAs(result);

            result.ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            secondMockBlock.Invocations.Count.ShouldBe(mockBlockInvocations.Length);
            foreach (var i in Enumerable.Range(0, mockBlockInvocations.Length))
                secondMockBlock.Invocations[i].ShouldBe(mockBlockInvocations[i]);

            testContext.MockLandingGearManager.Invocations.Count.ShouldBe(mockDoorManagerInvocations.Length);
            foreach (var i in Enumerable.Range(0, mockDoorManagerInvocations.Length))
                testContext.MockLandingGearManager.Invocations[i].ShouldBe(mockDoorManagerInvocations[i]);
        }

        #endregion MakeCollectBlockOperation Tests

        #region OnCompleted() Tests

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void OnCompleted_Always_LogsLandingGearCount(int landingGearCount)
        {
            var testContext = new TestContext()
            {
                LandingGearCount = landingGearCount
            };

            testContext.Uut.OnCompleted();

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.Contains(landingGearCount.ToString()))));
        }

        #endregion OnCompleted() Tests
    }
}
