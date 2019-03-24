using System.Linq;

using Moq;
using NUnit.Framework;
using Shouldly;

using Sandbox.ModAPI.Ingame;

using static IngameScript.Program;

namespace AutomatedDockingProcedures.Test
{
    [TestFixture]
    public class ConnectorManagerTests
    {
        #region Test Context

        private class TestContext
        {
            public TestContext()
            {
                Uut = new ConnectorManager();
            }

            public readonly ConnectorManager Uut;
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
                .Select(_ => new Mock<IMyShipConnector>())
                .ToArray();
            
            foreach(var mockExistingBlock in mockExistingBlocks)
                testContext.Uut.AddBlock(mockExistingBlock.Object);

            var mockBlock = new Mock<IMyShipConnector>();

            testContext.Uut.AddBlock(mockBlock.Object);

            testContext.Uut.Blocks.ShouldContain(mockBlock.Object);
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
                .Select(_ => new Mock<IMyShipConnector>())
                .ToArray();

            foreach (var mockBlock in mockBlocks)
                testContext.Uut.AddBlock(mockBlock.Object);

            testContext.Uut.ClearBlocks();

            testContext.Uut.Blocks.ShouldBeEmpty();
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
        public void MakeOnDockingOperation_BlocksIsNotEmpty_ConnectsEachBlock(int blockCount)
        {
            var testContext = new TestContext();

            var mockBlocks = Enumerable.Repeat(0, blockCount)
                .Select(_ => new Mock<IMyShipConnector>())
                .ToArray();

            foreach (var mockBlock in mockBlocks)
                testContext.Uut.AddBlock(mockBlock.Object);

            testContext.Uut.MakeOnDockingOperation()
                .ShouldRunToCompletionIn(blockCount);

            mockBlocks.ForEach(mockBlock =>
                mockBlock.ShouldHaveReceived(x => x.Connect()));
        }

        [TestCase(1)]
        [TestCase(10)]
        public void MakeOnDockingOperation_OperationIsDisposed_RecyclesOperation(int blockCount)
        {
            var testContext = new TestContext();

            var mockBlocks = Enumerable.Repeat(0, blockCount)
                .Select(_ => new Mock<IMyShipConnector>())
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
        public void MakeOnUndockingOperation_BlocksIsNotEmpty_DisconnectsEachBlock(int blockCount)
        {
            var testContext = new TestContext();

            var mockBlocks = Enumerable.Repeat(0, blockCount)
                .Select(_ => new Mock<IMyShipConnector>())
                .ToArray();

            foreach (var mockBlock in mockBlocks)
                testContext.Uut.AddBlock(mockBlock.Object);

            testContext.Uut.MakeOnUndockingOperation()
                .ShouldRunToCompletionIn(blockCount);

            mockBlocks.ForEach(mockBlock =>
                mockBlock.ShouldHaveReceived(x => x.Disconnect()));
        }

        [TestCase(1)]
        [TestCase(10)]
        public void MakeOnUndockingOperation_OperationIsDisposed_RecyclesOperation(int blockCount)
        {
            var testContext = new TestContext();

            var mockBlocks = Enumerable.Repeat(0, blockCount)
                .Select(_ => new Mock<IMyShipConnector>())
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
