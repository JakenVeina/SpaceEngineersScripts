using System.Linq;

using Moq;
using NUnit.Framework;
using Shouldly;
using static IngameScript.Program;

using Sandbox.ModAPI.Ingame;

namespace AutomatedDockingProcedures.Test
{
    [TestFixture]
    public class FunctionalBlockManagerTests
    {
        #region Test Context

        private class TestContext
        {
            public TestContext()
            {
                Uut = new FunctionalBlockManager();
            }

            public readonly FunctionalBlockManager Uut;
        }

        #endregion Test Context

        #region AddFunctionalBlock() Tests

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void AddFunctionalBlock_Always_AddsFunctionalBlock(int existingFunctionalBlockCount)
        {
            var testContext = new TestContext();

            var mockExistingFunctionalBlocks = Enumerable.Repeat(0, existingFunctionalBlockCount)
                .Select(_ => new Mock<IMyFunctionalBlock>())
                .ToArray();
            
            foreach(var mockExistingFunctionalBlock in mockExistingFunctionalBlocks)
                testContext.Uut.AddFunctionalBlock(mockExistingFunctionalBlock.Object);

            var mockFunctionalBlock = new Mock<IMyFunctionalBlock>();

            testContext.Uut.AddFunctionalBlock(mockFunctionalBlock.Object);

            testContext.Uut.FunctionalBlocks.ShouldContain(mockFunctionalBlock.Object);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void AddFunctionalBlock_BlockIsBeacon_IncrementsBeaconCount(int mockExistingBeaconCount)
        {
            var testContext = new TestContext();

            var mockExistingBeacons = Enumerable.Repeat(0, mockExistingBeaconCount)
                .Select(_ => new Mock<IMyBeacon>())
                .ToArray();

            foreach (var mockExistingBeacon in mockExistingBeacons)
                testContext.Uut.AddFunctionalBlock(mockExistingBeacon.Object);

            var mockBeacon = new Mock<IMyBeacon>();

            testContext.Uut.AddFunctionalBlock(mockBeacon.Object);

            testContext.Uut.BeaconCount.ShouldBe(mockExistingBeaconCount + 1);
            testContext.Uut.GasGeneratorCount.ShouldBe(0);
            testContext.Uut.GyroCount.ShouldBe(0);
            testContext.Uut.LightingBlockCount.ShouldBe(0);
            testContext.Uut.RadioAntennaCount.ShouldBe(0);
            testContext.Uut.ReactorCount.ShouldBe(0);
            testContext.Uut.ThrusterCount.ShouldBe(0);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void AddFunctionalBlock_BlockIsGasGenerator_IncrementsGasGeneratorCount(int mockExistingGasGeneratorCount)
        {
            var testContext = new TestContext();

            var mockExistingGasGenerators = Enumerable.Repeat(0, mockExistingGasGeneratorCount)
                .Select(_ => new Mock<IMyGasGenerator>())
                .ToArray();

            foreach (var mockExistingGasGenerator in mockExistingGasGenerators)
                testContext.Uut.AddFunctionalBlock(mockExistingGasGenerator.Object);

            var mockGasGenerator = new Mock<IMyGasGenerator>();

            testContext.Uut.AddFunctionalBlock(mockGasGenerator.Object);

            testContext.Uut.BeaconCount.ShouldBe(0);
            testContext.Uut.GasGeneratorCount.ShouldBe(mockExistingGasGeneratorCount + 1);
            testContext.Uut.GyroCount.ShouldBe(0);
            testContext.Uut.LightingBlockCount.ShouldBe(0);
            testContext.Uut.RadioAntennaCount.ShouldBe(0);
            testContext.Uut.ReactorCount.ShouldBe(0);
            testContext.Uut.ThrusterCount.ShouldBe(0);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void AddFunctionalBlock_BlockIsGyro_IncrementsGyroCount(int mockExistingGyroCount)
        {
            var testContext = new TestContext();

            var mockExistingGyros = Enumerable.Repeat(0, mockExistingGyroCount)
                .Select(_ => new Mock<IMyGyro>())
                .ToArray();

            foreach (var mockExistingGyro in mockExistingGyros)
                testContext.Uut.AddFunctionalBlock(mockExistingGyro.Object);

            var mockGyro = new Mock<IMyGyro>();

            testContext.Uut.AddFunctionalBlock(mockGyro.Object);

            testContext.Uut.BeaconCount.ShouldBe(0);
            testContext.Uut.GasGeneratorCount.ShouldBe(0);
            testContext.Uut.GyroCount.ShouldBe(mockExistingGyroCount + 1);
            testContext.Uut.LightingBlockCount.ShouldBe(0);
            testContext.Uut.RadioAntennaCount.ShouldBe(0);
            testContext.Uut.ReactorCount.ShouldBe(0);
            testContext.Uut.ThrusterCount.ShouldBe(0);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void AddFunctionalBlock_BlockIsLightingBlock_IncrementsLightingBlockCount(int mockExistingLightingBlockCount)
        {
            var testContext = new TestContext();

            var mockExistingLightingBlocks = Enumerable.Repeat(0, mockExistingLightingBlockCount)
                .Select(_ => new Mock<IMyLightingBlock>())
                .ToArray();

            foreach (var mockExistingLightingBlock in mockExistingLightingBlocks)
                testContext.Uut.AddFunctionalBlock(mockExistingLightingBlock.Object);

            var mockLightingBlock = new Mock<IMyLightingBlock>();

            testContext.Uut.AddFunctionalBlock(mockLightingBlock.Object);

            testContext.Uut.BeaconCount.ShouldBe(0);
            testContext.Uut.GasGeneratorCount.ShouldBe(0);
            testContext.Uut.GyroCount.ShouldBe(0);
            testContext.Uut.LightingBlockCount.ShouldBe(mockExistingLightingBlockCount + 1);
            testContext.Uut.RadioAntennaCount.ShouldBe(0);
            testContext.Uut.ReactorCount.ShouldBe(0);
            testContext.Uut.ThrusterCount.ShouldBe(0);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void AddFunctionalBlock_BlockIsRadioAntenna_IncrementsRadioAntennaCount(int mockExistingRadioAntennaCount)
        {
            var testContext = new TestContext();

            var mockExistingRadioAntennas = Enumerable.Repeat(0, mockExistingRadioAntennaCount)
                .Select(_ => new Mock<IMyRadioAntenna>())
                .ToArray();

            foreach (var mockExistingRadioAntenna in mockExistingRadioAntennas)
                testContext.Uut.AddFunctionalBlock(mockExistingRadioAntenna.Object);

            var mockRadioAntenna = new Mock<IMyRadioAntenna>();

            testContext.Uut.AddFunctionalBlock(mockRadioAntenna.Object);

            testContext.Uut.BeaconCount.ShouldBe(0);
            testContext.Uut.GasGeneratorCount.ShouldBe(0);
            testContext.Uut.GyroCount.ShouldBe(0);
            testContext.Uut.LightingBlockCount.ShouldBe(0);
            testContext.Uut.RadioAntennaCount.ShouldBe(mockExistingRadioAntennaCount + 1);
            testContext.Uut.ReactorCount.ShouldBe(0);
            testContext.Uut.ThrusterCount.ShouldBe(0);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void AddFunctionalBlock_BlockIsReactor_IncrementsReactorCount(int mockExistingReactorCount)
        {
            var testContext = new TestContext();

            var mockExistingReactors = Enumerable.Repeat(0, mockExistingReactorCount)
                .Select(_ => new Mock<IMyReactor>())
                .ToArray();

            foreach (var mockExistingReactor in mockExistingReactors)
                testContext.Uut.AddFunctionalBlock(mockExistingReactor.Object);

            var mockReactor = new Mock<IMyReactor>();

            testContext.Uut.AddFunctionalBlock(mockReactor.Object);

            testContext.Uut.BeaconCount.ShouldBe(0);
            testContext.Uut.GasGeneratorCount.ShouldBe(0);
            testContext.Uut.GyroCount.ShouldBe(0);
            testContext.Uut.LightingBlockCount.ShouldBe(0);
            testContext.Uut.RadioAntennaCount.ShouldBe(0);
            testContext.Uut.ReactorCount.ShouldBe(mockExistingReactorCount + 1);
            testContext.Uut.ThrusterCount.ShouldBe(0);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void AddFunctionalBlock_BlockIsThruster_IncrementsThrusterCount(int mockExistingThrusterCount)
        {
            var testContext = new TestContext();

            var mockExistingThrusters = Enumerable.Repeat(0, mockExistingThrusterCount)
                .Select(_ => new Mock<IMyThrust>())
                .ToArray();

            foreach (var mockExistingThruster in mockExistingThrusters)
                testContext.Uut.AddFunctionalBlock(mockExistingThruster.Object);

            var mockThruster = new Mock<IMyThrust>();

            testContext.Uut.AddFunctionalBlock(mockThruster.Object);

            testContext.Uut.BeaconCount.ShouldBe(0);
            testContext.Uut.GasGeneratorCount.ShouldBe(0);
            testContext.Uut.GyroCount.ShouldBe(0);
            testContext.Uut.LightingBlockCount.ShouldBe(0);
            testContext.Uut.RadioAntennaCount.ShouldBe(0);
            testContext.Uut.ReactorCount.ShouldBe(0);
            testContext.Uut.ThrusterCount.ShouldBe(mockExistingThrusterCount + 1);
        }

        #endregion AddFunctionalBlock() Tests

        #region ClearFunctionalBlocks() Tests

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void ClearFunctionalBlocks_Always_ClearsFunctionalBlocks(int functionalBlockCount)
        {
            var testContext = new TestContext();

            var mockFunctionalBlocks = Enumerable.Repeat(0, functionalBlockCount)
                .Select(_ => new Mock<IMyFunctionalBlock>())
                .ToArray();

            foreach (var mockFunctionalBlock in mockFunctionalBlocks)
                testContext.Uut.AddFunctionalBlock(mockFunctionalBlock.Object);

            testContext.Uut.ClearFunctionalBlocks();

            testContext.Uut.FunctionalBlocks.ShouldBeEmpty();
        }

        [Test]
        public void ClearFunctionalBlocks_Always_ClearsBlockCounts()
        {
            var testContext = new TestContext();

            testContext.Uut.AddFunctionalBlock(new Mock<IMyBeacon>().Object);
            testContext.Uut.AddFunctionalBlock(new Mock<IMyGasGenerator>().Object);
            testContext.Uut.AddFunctionalBlock(new Mock<IMyGyro>().Object);
            testContext.Uut.AddFunctionalBlock(new Mock<IMyLightingBlock>().Object);
            testContext.Uut.AddFunctionalBlock(new Mock<IMyRadioAntenna>().Object);
            testContext.Uut.AddFunctionalBlock(new Mock<IMyReactor>().Object);
            testContext.Uut.AddFunctionalBlock(new Mock<IMyThrust>().Object);

            testContext.Uut.ClearFunctionalBlocks();

            testContext.Uut.BeaconCount.ShouldBe(0);
            testContext.Uut.GasGeneratorCount.ShouldBe(0);
            testContext.Uut.GyroCount.ShouldBe(0);
            testContext.Uut.LightingBlockCount.ShouldBe(0);
            testContext.Uut.RadioAntennaCount.ShouldBe(0);
            testContext.Uut.ReactorCount.ShouldBe(0);
            testContext.Uut.ThrusterCount.ShouldBe(0);
        }

        #endregion ClearFunctionalBlocks() Tests

        #region MakeDisableOperation() Tests

        [Test]
        public void MakeDisableOperation_FunctionalBlocksIsEmpty_CompletesImmediately()
        {
            var testContext = new TestContext();

            testContext.Uut.MakeDisableOperation()
                .ShouldRunToCompletionIn(1);
        }

        [TestCase(1)]
        [TestCase(10)]
        public void MakeDisableOperation_FunctionalBlocksIsNotEmpty_DisablesEachFunctionalBlock(int functionalBlockCount)
        {
            var testContext = new TestContext();

            var mockFunctionalBlocks = Enumerable.Repeat(0, functionalBlockCount)
                .Select(_ => new Mock<IMyFunctionalBlock>())
                .ToArray();

            foreach (var mockFunctionalBlock in mockFunctionalBlocks)
                testContext.Uut.AddFunctionalBlock(mockFunctionalBlock.Object);

            testContext.Uut.MakeDisableOperation()
                .ShouldRunToCompletionIn(functionalBlockCount);

            mockFunctionalBlocks.ForEach(mockFunctionalBlock =>
                mockFunctionalBlock.ShouldHaveReceivedSet(x => x.Enabled = false));
        }

        [TestCase(1)]
        [TestCase(10)]
        public void MakeDisableOperation_OperationIsDisposed_RecyclesOperation(int functionalBlockCount)
        {
            var testContext = new TestContext();

            var mockFunctionalBlocks = Enumerable.Repeat(0, functionalBlockCount)
                .Select(_ => new Mock<IMyFunctionalBlock>())
                .ToArray();

            foreach (var mockFunctionalBlock in mockFunctionalBlocks)
                testContext.Uut.AddFunctionalBlock(mockFunctionalBlock.Object);

            var result = testContext.Uut.MakeDisableOperation();
            result.ShouldRunToCompletionIn(functionalBlockCount);

            var mockFunctionalBlockInvocations = mockFunctionalBlocks
                .Select(x => x.Invocations.ToArray())
                .ToArray();

            mockFunctionalBlocks.ForEach(x => x
                .Invocations.Clear());

            testContext.Uut.MakeDisableOperation()
                .ShouldBeSameAs(result);

            result.ShouldRunToCompletionIn(functionalBlockCount);

            foreach(var i in Enumerable.Range(0, mockFunctionalBlocks.Length))
            {
                mockFunctionalBlocks[i].Invocations.Count.ShouldBe(mockFunctionalBlockInvocations[i].Length);
                foreach (var j in Enumerable.Range(0, mockFunctionalBlocks[i].Invocations.Count))
                    mockFunctionalBlocks[i].Invocations[j].ShouldBe(mockFunctionalBlockInvocations[i][j]);
            }
        }

        #endregion MakeDisableOperation() Tests

        #region MakeEnableOperation() Tests

        [Test]
        public void MakeEnableOperation_FunctionalBlocksIsEmpty_CompletesImmediately()
        {
            var testContext = new TestContext();

            testContext.Uut.MakeEnableOperation()
                .ShouldRunToCompletionIn(1);
        }

        [TestCase(1)]
        [TestCase(10)]
        public void MakeEnableOperation_FunctionalBlocksIsNotEmpty_EnablesEachFunctionalBlock(int functionalBlockCount)
        {
            var testContext = new TestContext();

            var mockFunctionalBlocks = Enumerable.Repeat(0, functionalBlockCount)
                .Select(_ => new Mock<IMyFunctionalBlock>())
                .ToArray();

            foreach (var mockFunctionalBlock in mockFunctionalBlocks)
                testContext.Uut.AddFunctionalBlock(mockFunctionalBlock.Object);

            testContext.Uut.MakeEnableOperation()
                .ShouldRunToCompletionIn(functionalBlockCount);

            mockFunctionalBlocks.ForEach(mockFunctionalBlock =>
                mockFunctionalBlock.ShouldHaveReceivedSet(x => x.Enabled = true));
        }

        [TestCase(1)]
        [TestCase(10)]
        public void MakeEnableOperation_OperationIsDisposed_RecyclesOperation(int functionalBlockCount)
        {
            var testContext = new TestContext();

            var mockFunctionalBlocks = Enumerable.Repeat(0, functionalBlockCount)
                .Select(_ => new Mock<IMyFunctionalBlock>())
                .ToArray();

            foreach (var mockFunctionalBlock in mockFunctionalBlocks)
                testContext.Uut.AddFunctionalBlock(mockFunctionalBlock.Object);

            var result = testContext.Uut.MakeEnableOperation();
            result.ShouldRunToCompletionIn(functionalBlockCount);

            var mockFunctionalBlockInvocations = mockFunctionalBlocks
                .Select(x => x.Invocations.ToArray())
                .ToArray();

            mockFunctionalBlocks.ForEach(x => x
                .Invocations.Clear());

            testContext.Uut.MakeEnableOperation()
                .ShouldBeSameAs(result);

            result.ShouldRunToCompletionIn(functionalBlockCount);

            foreach (var i in Enumerable.Range(0, mockFunctionalBlocks.Length))
            {
                mockFunctionalBlocks[i].Invocations.Count.ShouldBe(mockFunctionalBlockInvocations[i].Length);
                foreach (var j in Enumerable.Range(0, mockFunctionalBlocks[i].Invocations.Count))
                    mockFunctionalBlocks[i].Invocations[j].ShouldBe(mockFunctionalBlockInvocations[i][j]);
            }
        }

        #endregion MakeEnableOperation() Tests
    }
}
