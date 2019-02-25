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
