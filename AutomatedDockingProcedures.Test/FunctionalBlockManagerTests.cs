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

        #region AddBlock() Tests

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void AddBlock_Always_AddsBlock(int existingBlockCount)
        {
            var testContext = new TestContext();

            var mockExistingBlocks = Enumerable.Repeat(0, existingBlockCount)
                .Select(_ => new Mock<IMyFunctionalBlock>())
                .ToArray();
            
            foreach(var mockExistingBlock in mockExistingBlocks)
                testContext.Uut.AddBlock(mockExistingBlock.Object);

            var mockBlock = new Mock<IMyFunctionalBlock>();

            testContext.Uut.AddBlock(mockBlock.Object);

            testContext.Uut.Blocks.ShouldContain(mockBlock.Object);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void AddBlock_BlockIsBeacon_IncrementsBeaconCount(int mockExistingBeaconCount)
        {
            var testContext = new TestContext();

            var mockExistingBeacons = Enumerable.Repeat(0, mockExistingBeaconCount)
                .Select(_ => new Mock<IMyBeacon>())
                .ToArray();

            foreach (var mockExistingBeacon in mockExistingBeacons)
                testContext.Uut.AddBlock(mockExistingBeacon.Object);

            var mockBeacon = new Mock<IMyBeacon>();

            testContext.Uut.AddBlock(mockBeacon.Object);

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
        public void AddBlock_BlockIsGasGenerator_IncrementsGasGeneratorCount(int mockExistingGasGeneratorCount)
        {
            var testContext = new TestContext();

            var mockExistingGasGenerators = Enumerable.Repeat(0, mockExistingGasGeneratorCount)
                .Select(_ => new Mock<IMyGasGenerator>())
                .ToArray();

            foreach (var mockExistingGasGenerator in mockExistingGasGenerators)
                testContext.Uut.AddBlock(mockExistingGasGenerator.Object);

            var mockGasGenerator = new Mock<IMyGasGenerator>();

            testContext.Uut.AddBlock(mockGasGenerator.Object);

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
        public void AddBlock_BlockIsGyro_IncrementsGyroCount(int mockExistingGyroCount)
        {
            var testContext = new TestContext();

            var mockExistingGyros = Enumerable.Repeat(0, mockExistingGyroCount)
                .Select(_ => new Mock<IMyGyro>())
                .ToArray();

            foreach (var mockExistingGyro in mockExistingGyros)
                testContext.Uut.AddBlock(mockExistingGyro.Object);

            var mockGyro = new Mock<IMyGyro>();

            testContext.Uut.AddBlock(mockGyro.Object);

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
        public void AddBlock_BlockIsLightingBlock_IncrementsLightingBlockCount(int mockExistingLightingBlockCount)
        {
            var testContext = new TestContext();

            var mockExistingLightingBlocks = Enumerable.Repeat(0, mockExistingLightingBlockCount)
                .Select(_ => new Mock<IMyLightingBlock>())
                .ToArray();

            foreach (var mockExistingLightingBlock in mockExistingLightingBlocks)
                testContext.Uut.AddBlock(mockExistingLightingBlock.Object);

            var mockLightingBlock = new Mock<IMyLightingBlock>();

            testContext.Uut.AddBlock(mockLightingBlock.Object);

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
        public void AddBlock_BlockIsRadioAntenna_IncrementsRadioAntennaCount(int mockExistingRadioAntennaCount)
        {
            var testContext = new TestContext();

            var mockExistingRadioAntennas = Enumerable.Repeat(0, mockExistingRadioAntennaCount)
                .Select(_ => new Mock<IMyRadioAntenna>())
                .ToArray();

            foreach (var mockExistingRadioAntenna in mockExistingRadioAntennas)
                testContext.Uut.AddBlock(mockExistingRadioAntenna.Object);

            var mockRadioAntenna = new Mock<IMyRadioAntenna>();

            testContext.Uut.AddBlock(mockRadioAntenna.Object);

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
        public void AddBlock_BlockIsReactor_IncrementsReactorCount(int mockExistingReactorCount)
        {
            var testContext = new TestContext();

            var mockExistingReactors = Enumerable.Repeat(0, mockExistingReactorCount)
                .Select(_ => new Mock<IMyReactor>())
                .ToArray();

            foreach (var mockExistingReactor in mockExistingReactors)
                testContext.Uut.AddBlock(mockExistingReactor.Object);

            var mockReactor = new Mock<IMyReactor>();

            testContext.Uut.AddBlock(mockReactor.Object);

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
        public void AddBlock_BlockIsThruster_IncrementsThrusterCount(int mockExistingThrusterCount)
        {
            var testContext = new TestContext();

            var mockExistingThrusters = Enumerable.Repeat(0, mockExistingThrusterCount)
                .Select(_ => new Mock<IMyThrust>())
                .ToArray();

            foreach (var mockExistingThruster in mockExistingThrusters)
                testContext.Uut.AddBlock(mockExistingThruster.Object);

            var mockThruster = new Mock<IMyThrust>();

            testContext.Uut.AddBlock(mockThruster.Object);

            testContext.Uut.BeaconCount.ShouldBe(0);
            testContext.Uut.GasGeneratorCount.ShouldBe(0);
            testContext.Uut.GyroCount.ShouldBe(0);
            testContext.Uut.LightingBlockCount.ShouldBe(0);
            testContext.Uut.RadioAntennaCount.ShouldBe(0);
            testContext.Uut.ReactorCount.ShouldBe(0);
            testContext.Uut.ThrusterCount.ShouldBe(mockExistingThrusterCount + 1);
        }

        #endregion AddBlock() Tests

        #region ClearBlocks() Tests

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void ClearBlocks_Always_ClearsBlocks(int blockCount)
        {
            var testContext = new TestContext();

            var mockBlocks = Enumerable.Repeat(0, blockCount)
                .Select(_ => new Mock<IMyFunctionalBlock>())
                .ToArray();

            foreach (var mockBlock in mockBlocks)
                testContext.Uut.AddBlock(mockBlock.Object);

            testContext.Uut.ClearBlocks();

            testContext.Uut.Blocks.ShouldBeEmpty();
        }

        [Test]
        public void ClearBlocks_Always_ClearsBlockCounts()
        {
            var testContext = new TestContext();

            testContext.Uut.AddBlock(new Mock<IMyBeacon>().Object);
            testContext.Uut.AddBlock(new Mock<IMyGasGenerator>().Object);
            testContext.Uut.AddBlock(new Mock<IMyGyro>().Object);
            testContext.Uut.AddBlock(new Mock<IMyLightingBlock>().Object);
            testContext.Uut.AddBlock(new Mock<IMyRadioAntenna>().Object);
            testContext.Uut.AddBlock(new Mock<IMyReactor>().Object);
            testContext.Uut.AddBlock(new Mock<IMyThrust>().Object);

            testContext.Uut.ClearBlocks();

            testContext.Uut.BeaconCount.ShouldBe(0);
            testContext.Uut.GasGeneratorCount.ShouldBe(0);
            testContext.Uut.GyroCount.ShouldBe(0);
            testContext.Uut.LightingBlockCount.ShouldBe(0);
            testContext.Uut.RadioAntennaCount.ShouldBe(0);
            testContext.Uut.ReactorCount.ShouldBe(0);
            testContext.Uut.ThrusterCount.ShouldBe(0);
        }

        #endregion ClearBlocks() Tests

        #region MakeOnDockingOperation() Tests

        [Test]
        public void MakeOnDockingOperation_BlocksIsEmpty_CompletesImmediately()
        {
            var testContext = new TestContext();

            testContext.Uut.MakeOnDockingOperation()
                .ShouldRunToCompletionIn(1);
        }

        [TestCase(1)]
        [TestCase(10)]
        public void MakeOnDockingOperation_BlocksIsNotEmpty_DisablesEachBlock(int blockCount)
        {
            var testContext = new TestContext();

            var mockBlocks = Enumerable.Repeat(0, blockCount)
                .Select(_ => new Mock<IMyFunctionalBlock>())
                .ToArray();

            foreach (var mockBlock in mockBlocks)
                testContext.Uut.AddBlock(mockBlock.Object);

            testContext.Uut.MakeOnDockingOperation()
                .ShouldRunToCompletionIn(blockCount);

            mockBlocks.ForEach(mockBlock =>
                mockBlock.ShouldHaveReceivedSet(x => x.Enabled = false));
        }

        [TestCase(1)]
        [TestCase(10)]
        public void MakeOnDockingOperation_OperationIsDisposed_RecyclesOperation(int blockCount)
        {
            var testContext = new TestContext();

            var mockBlocks = Enumerable.Repeat(0, blockCount)
                .Select(_ => new Mock<IMyFunctionalBlock>())
                .ToArray();

            foreach (var mockBlock in mockBlocks)
                testContext.Uut.AddBlock(mockBlock.Object);

            var result = testContext.Uut.MakeOnDockingOperation();
            result.ShouldRunToCompletionIn(blockCount);

            var mockBlockInvocations = mockBlocks
                .Select(x => x.Invocations.ToArray())
                .ToArray();

            mockBlocks.ForEach(x => x
                .Invocations.Clear());

            testContext.Uut.MakeOnDockingOperation()
                .ShouldBeSameAs(result);

            result.ShouldRunToCompletionIn(blockCount);

            foreach(var i in Enumerable.Range(0, mockBlocks.Length))
            {
                mockBlocks[i].Invocations.Count.ShouldBe(mockBlockInvocations[i].Length);
                foreach (var j in Enumerable.Range(0, mockBlocks[i].Invocations.Count))
                    mockBlocks[i].Invocations[j].ShouldBe(mockBlockInvocations[i][j]);
            }
        }

        #endregion MakeOnDockingOperation() Tests

        #region MakeOnUndockingOperation() Tests

        [Test]
        public void MakeOnUndockingOperation_BlocksIsEmpty_CompletesImmediately()
        {
            var testContext = new TestContext();

            testContext.Uut.MakeOnUndockingOperation()
                .ShouldRunToCompletionIn(1);
        }

        [TestCase(1)]
        [TestCase(10)]
        public void MakeOnUndockingOperation_BlocksIsNotEmpty_EnablesEachBlock(int blockCount)
        {
            var testContext = new TestContext();

            var mockBlocks = Enumerable.Repeat(0, blockCount)
                .Select(_ => new Mock<IMyFunctionalBlock>())
                .ToArray();

            foreach (var mockBlock in mockBlocks)
                testContext.Uut.AddBlock(mockBlock.Object);

            testContext.Uut.MakeOnUndockingOperation()
                .ShouldRunToCompletionIn(blockCount);

            mockBlocks.ForEach(mockBlock =>
                mockBlock.ShouldHaveReceivedSet(x => x.Enabled = true));
        }

        [TestCase(1)]
        [TestCase(10)]
        public void MakeOnUndockingOperation_OperationIsDisposed_RecyclesOperation(int blockCount)
        {
            var testContext = new TestContext();

            var mockBlocks = Enumerable.Repeat(0, blockCount)
                .Select(_ => new Mock<IMyFunctionalBlock>())
                .ToArray();

            foreach (var mockBlock in mockBlocks)
                testContext.Uut.AddBlock(mockBlock.Object);

            var result = testContext.Uut.MakeOnUndockingOperation();
            result.ShouldRunToCompletionIn(blockCount);

            var mockBlockInvocations = mockBlocks
                .Select(x => x.Invocations.ToArray())
                .ToArray();

            mockBlocks.ForEach(x => x
                .Invocations.Clear());

            testContext.Uut.MakeOnUndockingOperation()
                .ShouldBeSameAs(result);

            result.ShouldRunToCompletionIn(blockCount);

            foreach (var i in Enumerable.Range(0, mockBlocks.Length))
            {
                mockBlocks[i].Invocations.Count.ShouldBe(mockBlockInvocations[i].Length);
                foreach (var j in Enumerable.Range(0, mockBlocks[i].Invocations.Count))
                    mockBlocks[i].Invocations[j].ShouldBe(mockBlockInvocations[i][j]);
            }
        }

        #endregion MakeOnUndockingOperation() Tests
    }
}
