using System.Linq;

using Moq;
using NUnit.Framework;
using Shouldly;
using static IngameScript.Program;

using Sandbox.ModAPI.Ingame;

namespace AutomatedDockingProcedures.Test
{
    [TestFixture]
    public class BatteryBlockManagerTests
    {
        #region Test Context

        private class TestContext
        {
            public TestContext()
            {
                Uut = new BatteryBlockManager();
            }

            public readonly BatteryBlockManager Uut;
        }

        #endregion Test Context

        #region AddBatteryBlock() Tests

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void AddBatteryBlock_Always_AddsBatteryBlock(int existingBatteryBlockCount)
        {
            var testContext = new TestContext();

            var mockExistingBatteryBlocks = Enumerable.Repeat(0, existingBatteryBlockCount)
                .Select(_ => new Mock<IMyBatteryBlock>())
                .ToArray();
            
            foreach(var mockExistingBatteryBlock in mockExistingBatteryBlocks)
                testContext.Uut.AddBatteryBlock(mockExistingBatteryBlock.Object);

            var mockBatteryBlock = new Mock<IMyBatteryBlock>();

            testContext.Uut.AddBatteryBlock(mockBatteryBlock.Object);

            testContext.Uut.BatteryBlocks.ShouldContain(mockBatteryBlock.Object);
        }

        #endregion AddBatteryBlock() Tests

        #region ClearBatteryBlocks() Tests

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void ClearBatteryBlocks_Always_ClearsBatteryBlocks(int batteryBlockCount)
        {
            var testContext = new TestContext();

            var mockBatteryBlocks = Enumerable.Repeat(0, batteryBlockCount)
                .Select(_ => new Mock<IMyBatteryBlock>())
                .ToArray();

            foreach (var mockBatteryBlock in mockBatteryBlocks)
                testContext.Uut.AddBatteryBlock(mockBatteryBlock.Object);

            testContext.Uut.ClearBatteryBlocks();

            testContext.Uut.BatteryBlocks.ShouldBeEmpty();
        }

        #endregion ClearBatteryBlocks() Tests

        #region MakeRechargeOperation() Tests

        [Test]
        public void MakeRechargeOperation_BatteryBlocksIsEmpty_CompletesImmediately()
        {
            var testContext = new TestContext();

            testContext.Uut.MakeRechargeOperation()
                .ShouldRunToCompletionIn(1);
        }

        [TestCase(1)]
        [TestCase(10)]
        public void MakeRechargeOperation_BatteryBlocksIsNotEmpty_RechargesEachBatteryBlock(int batteryBlockCount)
        {
            var testContext = new TestContext();

            var mockBatteryBlocks = Enumerable.Repeat(0, batteryBlockCount)
                .Select(_ => new Mock<IMyBatteryBlock>())
                .ToArray();

            foreach (var mockBatteryBlock in mockBatteryBlocks)
                testContext.Uut.AddBatteryBlock(mockBatteryBlock.Object);

            testContext.Uut.MakeRechargeOperation()
                .ShouldRunToCompletionIn(batteryBlockCount);

            mockBatteryBlocks.ForEach(mockBatteryBlock =>
                mockBatteryBlock.ShouldHaveReceivedSet(x => x.ChargeMode = ChargeMode.Recharge));
        }

        [TestCase(1)]
        [TestCase(10)]
        public void MakeRechargeOperation_OperationIsDisposed_RecyclesOperation(int batteryBlockCount)
        {
            var testContext = new TestContext();

            var mockBatteryBlocks = Enumerable.Repeat(0, batteryBlockCount)
                .Select(_ => new Mock<IMyBatteryBlock>())
                .ToArray();

            foreach (var mockBatteryBlock in mockBatteryBlocks)
                testContext.Uut.AddBatteryBlock(mockBatteryBlock.Object);

            var result = testContext.Uut.MakeRechargeOperation();
            result.ShouldRunToCompletionIn(batteryBlockCount);

            var mockBatteryBlockInvocations = mockBatteryBlocks
                .Select(x => x.Invocations.ToArray())
                .ToArray();

            mockBatteryBlocks.ForEach(x => x
                .Invocations.Clear());

            testContext.Uut.MakeRechargeOperation()
                .ShouldBeSameAs(result);

            result.ShouldRunToCompletionIn(batteryBlockCount);

            foreach(var i in Enumerable.Range(0, mockBatteryBlocks.Length))
            {
                mockBatteryBlocks[i].Invocations.Count.ShouldBe(mockBatteryBlockInvocations[i].Length);
                foreach (var j in Enumerable.Range(0, mockBatteryBlocks[i].Invocations.Count))
                    mockBatteryBlocks[i].Invocations[j].ShouldBe(mockBatteryBlockInvocations[i][j]);
            }
        }

        #endregion MakeRechargeOperation() Tests

        #region MakeDischargeOperation() Tests

        [Test]
        public void MakeDischargeOperation_BatteryBlocksIsEmpty_CompletesImmediately()
        {
            var testContext = new TestContext();

            testContext.Uut.MakeDischargeOperation()
                .ShouldRunToCompletionIn(1);
        }

        [TestCase(1)]
        [TestCase(10)]
        public void MakeDischargeOperation_BatteryBlocksIsNotEmpty_DischargesEachBatteryBlock(int batteryBlockCount)
        {
            var testContext = new TestContext();

            var mockBatteryBlocks = Enumerable.Repeat(0, batteryBlockCount)
                .Select(_ => new Mock<IMyBatteryBlock>())
                .ToArray();

            foreach (var mockBatteryBlock in mockBatteryBlocks)
                testContext.Uut.AddBatteryBlock(mockBatteryBlock.Object);

            testContext.Uut.MakeDischargeOperation()
                .ShouldRunToCompletionIn(batteryBlockCount);

            mockBatteryBlocks.ForEach(mockBatteryBlock =>
                mockBatteryBlock.ShouldHaveReceivedSet(x => x.ChargeMode = ChargeMode.Discharge));
        }

        [TestCase(1)]
        [TestCase(10)]
        public void MakeDischargeOperation_OperationIsDisposed_RecyclesOperation(int batteryBlockCount)
        {
            var testContext = new TestContext();

            var mockBatteryBlocks = Enumerable.Repeat(0, batteryBlockCount)
                .Select(_ => new Mock<IMyBatteryBlock>())
                .ToArray();

            foreach (var mockBatteryBlock in mockBatteryBlocks)
                testContext.Uut.AddBatteryBlock(mockBatteryBlock.Object);

            var result = testContext.Uut.MakeDischargeOperation();
            result.ShouldRunToCompletionIn(batteryBlockCount);

            var mockBatteryBlockInvocations = mockBatteryBlocks
                .Select(x => x.Invocations.ToArray())
                .ToArray();

            mockBatteryBlocks.ForEach(x => x
                .Invocations.Clear());

            testContext.Uut.MakeDischargeOperation()
                .ShouldBeSameAs(result);

            result.ShouldRunToCompletionIn(batteryBlockCount);

            foreach (var i in Enumerable.Range(0, mockBatteryBlocks.Length))
            {
                mockBatteryBlocks[i].Invocations.Count.ShouldBe(mockBatteryBlockInvocations[i].Length);
                foreach (var j in Enumerable.Range(0, mockBatteryBlocks[i].Invocations.Count))
                    mockBatteryBlocks[i].Invocations[j].ShouldBe(mockBatteryBlockInvocations[i][j]);
            }
        }

        #endregion MakeDischargeOperation() Tests
    }
}
