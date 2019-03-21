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
            public int BeaconCount;

            public int GasGeneratorCount;

            public int GyroCount;

            public int LightingBlockCount;

            public int RadioAntennaCount;

            public int ReactorCount;

            public int ThrusterCount;

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

            public bool IgnoreThrusters
                = false;

            public bool IgnoreOtherBlocks
                = false;

            public TestContext()
            {
                MockFunctionalBlockManager = new Mock<IFunctionalBlockManager>();
                MockFunctionalBlockManager
                    .Setup(x => x.BeaconCount)
                    .Returns(() => BeaconCount);
                MockFunctionalBlockManager
                    .Setup(x => x.GasGeneratorCount)
                    .Returns(() => GasGeneratorCount);
                MockFunctionalBlockManager
                    .Setup(x => x.GyroCount)
                    .Returns(() => GyroCount);
                MockFunctionalBlockManager
                    .Setup(x => x.LightingBlockCount)
                    .Returns(() => LightingBlockCount);
                MockFunctionalBlockManager
                    .Setup(x => x.RadioAntennaCount)
                    .Returns(() => RadioAntennaCount);
                MockFunctionalBlockManager
                    .Setup(x => x.ReactorCount)
                    .Returns(() => ReactorCount);
                MockFunctionalBlockManager
                    .Setup(x => x.ThrusterCount)
                    .Returns(() => ThrusterCount);

                MockLogger = new Mock<ILogger>();

                MockDockingManagerSettingsProvider = new Mock<IDockingManagerSettingsProvider>();
                MockDockingManagerSettingsProvider
                    .Setup(x => x.Settings)
                    .Returns(() => new DockingManagerSettings()
                    {
                        IgnoreBatteryBlocks = IgnoreOtherBlocks,
                        IgnoreBeacons = IgnoreBeacons,
                        IgnoreGasGenerators = IgnoreGasGenerators,
                        IgnoreGasTanks = IgnoreOtherBlocks,
                        IgnoreGyros = IgnoreGyros,
                        IgnoreLandingGears = IgnoreOtherBlocks,
                        IgnoreLightingBlocks = IgnoreLightingBlocks,
                        IgnoreRadioAntennae = IgnoreRadioAntennae,
                        IgnoreReactors = IgnoreReactors,
                        IgnoreThrusters = IgnoreThrusters
                    });

                Uut = new FunctionalBlockCollectionHandler(
                    MockFunctionalBlockManager.Object,
                    MockLogger.Object,
                    MockDockingManagerSettingsProvider.Object);

                MockBackgroundWorker = new FakeBackgroundWorker();
            }

            public readonly Mock<IFunctionalBlockManager> MockFunctionalBlockManager;

            public readonly Mock<ILogger> MockLogger;

            public readonly Mock<IDockingManagerSettingsProvider> MockDockingManagerSettingsProvider;

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

        [TestCase(typeof(IMyBeacon),        false, true,  true,  true,  true,  true,  true,  true )]
        [TestCase(typeof(IMyBeacon),        false, false, false, false, false, false, false, false)]
        [TestCase(typeof(IMyGasGenerator),  true,  false, true,  true,  true,  true,  true,  true )]
        [TestCase(typeof(IMyGasGenerator),  false, false, false, false, false, false, false, false)]
        [TestCase(typeof(IMyGyro),          true,  true,  false, true,  true,  true,  true,  true )]
        [TestCase(typeof(IMyGyro),          false, false, false, false, false, false, false, false)]
        [TestCase(typeof(IMyLightingBlock), true,  true,  true,  false, true,  true,  true,  true )]
        [TestCase(typeof(IMyLightingBlock), false, false, false, false, false, false, false, false)]
        [TestCase(typeof(IMyRadioAntenna),  true,  true,  true,  true,  false, true,  true,  true )]
        [TestCase(typeof(IMyRadioAntenna),  false, false, false, false, false, false, false, false)]
        [TestCase(typeof(IMyReactor),       true,  true,  true,  true,  true,  false, true,  true )]
        [TestCase(typeof(IMyReactor),       false, false, false, false, false, false, false, false)]
        [TestCase(typeof(IMyThrust),        true,  true,  true,  true,  true,  true,  false, true )]
        [TestCase(typeof(IMyThrust),        false, false, false, false, false, false, false, false)]
        public void MakeCollectBlockOperation_BlockIsRelevantFunctionalBlock_AddsFunctionalBlock(
            Type blockType,
            bool ignoreBeacons,
            bool ignoreGasGenerators,
            bool ignoreGyros,
            bool ignoreLightingBlocks,
            bool ignoreRadioAntennae,
            bool ignoreReactors,
            bool ignoreThrusters,
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
                IgnoreThrusters = ignoreThrusters,
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

        [TestCase(typeof(IMyBeacon),        true,  false, false, false, false, false, false)]
        [TestCase(typeof(IMyGasGenerator),  false, true,  false, false, false, false, false)]
        [TestCase(typeof(IMyGyro),          false, false, true,  false, false, false, false)]
        [TestCase(typeof(IMyLightingBlock), false, false, false, true,  false, false, false)]
        [TestCase(typeof(IMyRadioAntenna),  false, false, false, false, true,  false, false)]
        [TestCase(typeof(IMyReactor),       false, false, false, false, false, true,  false)]
        [TestCase(typeof(IMyThrust),        false, false, false, false, false, false, true )]
        public void MakeCollectBlockOperation_BlockTypeIsIgnored_IgnoresBlock(
            Type blockType,
            bool ignoreBeacons,
            bool ignoreGasGenerators,
            bool ignoreGyros,
            bool ignoreLightingBlocks,
            bool ignoreRadioAntennae,
            bool ignoreReactors,
            bool ignoreThrusters)
        {
            var testContext = new TestContext()
            {
                IgnoreBeacons = ignoreBeacons,
                IgnoreGasGenerators = ignoreGasGenerators,
                IgnoreGyros = ignoreGyros,
                IgnoreLightingBlocks = ignoreLightingBlocks,
                IgnoreRadioAntennae = ignoreRadioAntennae,
                IgnoreReactors = ignoreReactors,
                IgnoreThrusters = ignoreThrusters
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
        [TestCase(typeof(IMyThrust))]
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

        [Test]
        public void OnCompleted_BeaconCountIs0_DoesNotLogBeaconCount()
        {
            var testContext = new TestContext()
            {
                BeaconCount = 0
            };

            testContext.Uut.OnCompleted();

            testContext.MockLogger
                .ShouldNotHaveReceived(x => x.AddLine(It.IsAny<string>()));
        }

        [TestCase(1)]
        [TestCase(10)]
        public void OnCompleted_BeaconCountIsGreaterThan0_LogsBeaconCount(int beaconCount)
        {
            var testContext = new TestContext()
            {
                BeaconCount = beaconCount
            };

            testContext.Uut.OnCompleted();

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.Contains(beaconCount.ToString()))));
        }

        [Test]
        public void OnCompleted_GasGeneratorCountIs0_DoesNotLogGasGeneratorCount()
        {
            var testContext = new TestContext()
            {
                GasGeneratorCount = 0
            };

            testContext.Uut.OnCompleted();

            testContext.MockLogger
                .ShouldNotHaveReceived(x => x.AddLine(It.IsAny<string>()));
        }

        [TestCase(1)]
        [TestCase(10)]
        public void OnCompleted_GasGeneratorCountIsGreaterThan0_LogsGasGeneratorCount(int gasGeneratorCount)
        {
            var testContext = new TestContext()
            {
                GasGeneratorCount = gasGeneratorCount
            };

            testContext.Uut.OnCompleted();

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.Contains(gasGeneratorCount.ToString()))));
        }

        [Test]
        public void OnCompleted_GyroCountIs0_DoesNotLogGyroCount()
        {
            var testContext = new TestContext()
            {
                GyroCount = 0
            };

            testContext.Uut.OnCompleted();

            testContext.MockLogger
                .ShouldNotHaveReceived(x => x.AddLine(It.IsAny<string>()));
        }

        [TestCase(1)]
        [TestCase(10)]
        public void OnCompleted_GyroCountIsGreaterThan0_LogsGyroCount(int gyroCount)
        {
            var testContext = new TestContext()
            {
                GyroCount = gyroCount
            };

            testContext.Uut.OnCompleted();

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.Contains(gyroCount.ToString()))));
        }

        [Test]
        public void OnCompleted_LightingBlockCountIs0_DoesNotLogLightingBlockCount()
        {
            var testContext = new TestContext()
            {
                LightingBlockCount = 0
            };

            testContext.Uut.OnCompleted();

            testContext.MockLogger
                .ShouldNotHaveReceived(x => x.AddLine(It.IsAny<string>()));
        }

        [TestCase(1)]
        [TestCase(10)]
        public void OnCompleted_LightingBlockCountIsGreaterThan0_LogsLightingBlockCount(int lightingBlockCount)
        {
            var testContext = new TestContext()
            {
                LightingBlockCount = lightingBlockCount
            };

            testContext.Uut.OnCompleted();

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.Contains(lightingBlockCount.ToString()))));
        }

        [Test]
        public void OnCompleted_RadioAntennaCountIs0_DoesNotLogRadioAntennaCount()
        {
            var testContext = new TestContext()
            {
                RadioAntennaCount = 0
            };

            testContext.Uut.OnCompleted();

            testContext.MockLogger
                .ShouldNotHaveReceived(x => x.AddLine(It.IsAny<string>()));
        }

        [TestCase(1)]
        [TestCase(10)]
        public void OnCompleted_RadioAntennaCountIsGreaterThan0_LogsRadioAntennaCount(int radioAntennaCount)
        {
            var testContext = new TestContext()
            {
                RadioAntennaCount = radioAntennaCount
            };

            testContext.Uut.OnCompleted();

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.Contains(radioAntennaCount.ToString()))));
        }

        [Test]
        public void OnCompleted_ReactorCountIs0_DoesNotLogReactorCount()
        {
            var testContext = new TestContext()
            {
                ReactorCount = 0
            };

            testContext.Uut.OnCompleted();

            testContext.MockLogger
                .ShouldNotHaveReceived(x => x.AddLine(It.IsAny<string>()));
        }

        [TestCase(1)]
        [TestCase(10)]
        public void OnCompleted_ReactorCountIsGreaterThan0_LogsReactorCount(int reactorCount)
        {
            var testContext = new TestContext()
            {
                ReactorCount = reactorCount
            };

            testContext.Uut.OnCompleted();

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.Contains(reactorCount.ToString()))));
        }

        [Test]
        public void OnCompleted_ThrusterCountIs0_DoesNotLogThrusterCount()
        {
            var testContext = new TestContext()
            {
                ThrusterCount = 0
            };

            testContext.Uut.OnCompleted();

            testContext.MockLogger
                .ShouldNotHaveReceived(x => x.AddLine(It.IsAny<string>()));
        }

        [TestCase(1)]
        [TestCase(10)]
        public void OnCompleted_ThrusterCountIsGreaterThan0_LogsThrusterCount(int thrusterCount)
        {
            var testContext = new TestContext()
            {
                ThrusterCount = thrusterCount
            };

            testContext.Uut.OnCompleted();

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.Contains(thrusterCount.ToString()))));
        }

        [TestCase(1, 2, 3, 4, 5, 6, 7)]
        public void OnCompleted_FunctionalBlockCountsAreAllGreaterThan0_LogsAllFunctionalBlockCounts(
            int beaconCount,
            int gasGeneratorCount,
            int gyroCount,
            int lightingBlockCount,
            int radioAntennaCount,
            int reactorCount,
            int thrusterCount)
        {
            var testContext = new TestContext()
            {
                BeaconCount = beaconCount,
                GasGeneratorCount = gasGeneratorCount,
                GyroCount = gyroCount,
                LightingBlockCount = lightingBlockCount,
                RadioAntennaCount = radioAntennaCount,
                ReactorCount = reactorCount,
                ThrusterCount = thrusterCount
            };

            testContext.Uut.OnCompleted();

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.Contains(beaconCount.ToString()))));
            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.Contains(gasGeneratorCount.ToString()))));
            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.Contains(gyroCount.ToString()))));
            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.Contains(lightingBlockCount.ToString()))));
            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.Contains(radioAntennaCount.ToString()))));
            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.Contains(reactorCount.ToString()))));
            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.Contains(thrusterCount.ToString()))));
        }

        #endregion OnCompleted() Tests
    }
}
