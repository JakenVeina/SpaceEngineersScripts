using System.Linq;

using Moq;
using NUnit.Framework;
using Shouldly;
using static IngameScript.Program;

using Sandbox.ModAPI.Ingame;

namespace AutomatedDockingProcedures.Test
{
    [TestFixture]
    public class GasTankManagerTests
    {
        #region Test Context

        private class TestContext
        {
            public TestContext()
            {
                Uut = new GasTankManager();
            }

            public readonly GasTankManager Uut;
        }

        #endregion Test Context

        #region AddGasTank() Tests

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void AddGasTank_Always_AddsGasTank(int existingGasTankCount)
        {
            var testContext = new TestContext();

            var mockExistingGasTanks = Enumerable.Repeat(0, existingGasTankCount)
                .Select(_ => new Mock<IMyGasTank>())
                .ToArray();
            
            foreach(var mockExistingGasTank in mockExistingGasTanks)
                testContext.Uut.AddGasTank(mockExistingGasTank.Object);

            var mockGasTank = new Mock<IMyGasTank>();

            testContext.Uut.AddGasTank(mockGasTank.Object);

            testContext.Uut.GasTanks.ShouldContain(mockGasTank.Object);
        }

        #endregion AddGasTank() Tests

        #region ClearGasTanks() Tests

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void ClearGasTanks_Always_ClearsGasTanks(int gasTankCount)
        {
            var testContext = new TestContext();

            var mockGasTanks = Enumerable.Repeat(0, gasTankCount)
                .Select(_ => new Mock<IMyGasTank>())
                .ToArray();

            foreach (var mockGasTank in mockGasTanks)
                testContext.Uut.AddGasTank(mockGasTank.Object);

            testContext.Uut.ClearGasTanks();

            testContext.Uut.GasTanks.ShouldBeEmpty();
        }

        #endregion ClearGasTanks() Tests

        #region MakeStockpileOperation() Tests

        [Test]
        public void MakeStockpileOperation_GasTanksIsEmpty_CompletesImmediately()
        {
            var testContext = new TestContext();

            testContext.Uut.MakeStockpileOperation()
                .ShouldRunToCompletionIn(1);
        }

        [TestCase(1)]
        [TestCase(10)]
        public void MakeStockpileOperation_GasTanksIsNotEmpty_StockpilesEachGasTank(int gasTankCount)
        {
            var testContext = new TestContext();

            var mockGasTanks = Enumerable.Repeat(0, gasTankCount)
                .Select(_ => new Mock<IMyGasTank>())
                .ToArray();

            foreach (var mockGasTank in mockGasTanks)
                testContext.Uut.AddGasTank(mockGasTank.Object);

            testContext.Uut.MakeStockpileOperation()
                .ShouldRunToCompletionIn(gasTankCount);

            mockGasTanks.ForEach(mockGasTank =>
                mockGasTank.ShouldHaveReceivedSet(x => x.Stockpile = true));
        }

        [TestCase(1)]
        [TestCase(10)]
        public void MakeStockpileOperation_OperationIsDisposed_RecyclesOperation(int gasTankCount)
        {
            var testContext = new TestContext();

            var mockGasTanks = Enumerable.Repeat(0, gasTankCount)
                .Select(_ => new Mock<IMyGasTank>())
                .ToArray();

            foreach (var mockGasTank in mockGasTanks)
                testContext.Uut.AddGasTank(mockGasTank.Object);

            var result = testContext.Uut.MakeStockpileOperation();
            result.ShouldRunToCompletionIn(gasTankCount);

            var mockGasTankInvocations = mockGasTanks
                .Select(x => x.Invocations.ToArray())
                .ToArray();

            mockGasTanks.ForEach(x => x
                .Invocations.Clear());

            testContext.Uut.MakeStockpileOperation()
                .ShouldBeSameAs(result);

            result.ShouldRunToCompletionIn(gasTankCount);

            foreach(var i in Enumerable.Range(0, mockGasTanks.Length))
            {
                mockGasTanks[i].Invocations.Count.ShouldBe(mockGasTankInvocations[i].Length);
                foreach (var j in Enumerable.Range(0, mockGasTanks[i].Invocations.Count))
                    mockGasTanks[i].Invocations[j].ShouldBe(mockGasTankInvocations[i][j]);
            }
        }

        #endregion MakeStockpileOperation() Tests

        #region MakeDispenseOperation() Tests

        [Test]
        public void MakeDispenseOperation_GasTanksIsEmpty_CompletesImmediately()
        {
            var testContext = new TestContext();

            testContext.Uut.MakeDispenseOperation()
                .ShouldRunToCompletionIn(1);
        }

        [TestCase(1)]
        [TestCase(10)]
        public void MakeDispenseOperation_GasTanksIsNotEmpty_DispensesEachGasTank(int gasTankCount)
        {
            var testContext = new TestContext();

            var mockGasTanks = Enumerable.Repeat(0, gasTankCount)
                .Select(_ => new Mock<IMyGasTank>())
                .ToArray();

            foreach (var mockGasTank in mockGasTanks)
                testContext.Uut.AddGasTank(mockGasTank.Object);

            testContext.Uut.MakeDispenseOperation()
                .ShouldRunToCompletionIn(gasTankCount);

            mockGasTanks.ForEach(mockGasTank =>
                mockGasTank.ShouldHaveReceivedSet(x => x.Stockpile = false));
        }

        [TestCase(1)]
        [TestCase(10)]
        public void MakeDispenseOperation_OperationIsDisposed_RecyclesOperation(int gasTankCount)
        {
            var testContext = new TestContext();

            var mockGasTanks = Enumerable.Repeat(0, gasTankCount)
                .Select(_ => new Mock<IMyGasTank>())
                .ToArray();

            foreach (var mockGasTank in mockGasTanks)
                testContext.Uut.AddGasTank(mockGasTank.Object);

            var result = testContext.Uut.MakeDispenseOperation();
            result.ShouldRunToCompletionIn(gasTankCount);

            var mockGasTankInvocations = mockGasTanks
                .Select(x => x.Invocations.ToArray())
                .ToArray();

            mockGasTanks.ForEach(x => x
                .Invocations.Clear());

            testContext.Uut.MakeDispenseOperation()
                .ShouldBeSameAs(result);

            result.ShouldRunToCompletionIn(gasTankCount);

            foreach (var i in Enumerable.Range(0, mockGasTanks.Length))
            {
                mockGasTanks[i].Invocations.Count.ShouldBe(mockGasTankInvocations[i].Length);
                foreach (var j in Enumerable.Range(0, mockGasTanks[i].Invocations.Count))
                    mockGasTanks[i].Invocations[j].ShouldBe(mockGasTankInvocations[i][j]);
            }
        }

        #endregion MakeDispenseOperation() Tests
    }
}
