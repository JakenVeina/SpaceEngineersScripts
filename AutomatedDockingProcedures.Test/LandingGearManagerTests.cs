using System.Linq;

using Moq;
using NUnit.Framework;
using Shouldly;
using static IngameScript.Program;

using SpaceEngineers.Game.ModAPI.Ingame;

namespace AutomatedDockingProcedures.Test
{
    [TestFixture]
    public class LandingGearManagerTests
    {
        #region Test Context

        private class TestContext
        {
            public TestContext()
            {
                Uut = new LandingGearManager();
            }

            public readonly LandingGearManager Uut;
        }

        #endregion Test Context

        #region AddLandingGear() Tests

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void AddLandingGear_Always_AddsLandingGear(int existingLandingGearCount)
        {
            var testContext = new TestContext();

            var mockExistingLandingGears = Enumerable.Repeat(0, existingLandingGearCount)
                .Select(_ => new Mock<IMyLandingGear>())
                .ToArray();
            
            foreach(var mockExistingLandingGear in mockExistingLandingGears)
                testContext.Uut.AddLandingGear(mockExistingLandingGear.Object);

            var mockLandingGear = new Mock<IMyLandingGear>();

            testContext.Uut.AddLandingGear(mockLandingGear.Object);

            testContext.Uut.LandingGears.ShouldContain(mockLandingGear.Object);
        }

        #endregion AddLandingGear() Tests

        #region ClearLandingGears() Tests

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void ClearLandingGears_Always_ClearsLandingGears(int landingGearCount)
        {
            var testContext = new TestContext();

            var mockLandingGears = Enumerable.Repeat(0, landingGearCount)
                .Select(_ => new Mock<IMyLandingGear>())
                .ToArray();

            foreach (var mockLandingGear in mockLandingGears)
                testContext.Uut.AddLandingGear(mockLandingGear.Object);

            testContext.Uut.ClearLandingGears();

            testContext.Uut.LandingGears.ShouldBeEmpty();
        }

        #endregion ClearLandingGears() Tests

        #region MakeLockOperation() Tests

        [Test]
        public void MakeLockOperation_LandingGearsIsEmpty_CompletesImmediately()
        {
            var testContext = new TestContext();

            testContext.Uut.MakeLockOperation()
                .ShouldRunToCompletionIn(1);
        }

        [TestCase(1)]
        [TestCase(10)]
        public void MakeLockOperation_LandingGearsIsNotEmpty_LocksEachLandingGear(int landingGearCount)
        {
            var testContext = new TestContext();

            var mockLandingGears = Enumerable.Repeat(0, landingGearCount)
                .Select(_ => new Mock<IMyLandingGear>())
                .ToArray();

            foreach (var mockLandingGear in mockLandingGears)
                testContext.Uut.AddLandingGear(mockLandingGear.Object);

            testContext.Uut.MakeLockOperation()
                .ShouldRunToCompletionIn(landingGearCount);

            mockLandingGears.ForEach(mockLandingGear =>
                mockLandingGear.ShouldHaveReceived(x => x.Lock()));
        }

        [TestCase(1)]
        [TestCase(10)]
        public void MakeLockOperation_OperationIsDisposed_RecyclesOperation(int landingGearCount)
        {
            var testContext = new TestContext();

            var mockLandingGears = Enumerable.Repeat(0, landingGearCount)
                .Select(_ => new Mock<IMyLandingGear>())
                .ToArray();

            foreach (var mockLandingGear in mockLandingGears)
                testContext.Uut.AddLandingGear(mockLandingGear.Object);

            var result = testContext.Uut.MakeLockOperation();
            result.ShouldRunToCompletionIn(landingGearCount);

            var mockLandingGearInvocations = mockLandingGears
                .Select(x => x.Invocations.ToArray())
                .ToArray();

            mockLandingGears.ForEach(x => x
                .Invocations.Clear());

            testContext.Uut.MakeLockOperation()
                .ShouldBeSameAs(result);

            result.ShouldRunToCompletionIn(landingGearCount);

            foreach(var i in Enumerable.Range(0, mockLandingGears.Length))
            {
                mockLandingGears[i].Invocations.Count.ShouldBe(mockLandingGearInvocations[i].Length);
                foreach (var j in Enumerable.Range(0, mockLandingGears[i].Invocations.Count))
                    mockLandingGears[i].Invocations[j].ShouldBe(mockLandingGearInvocations[i][j]);
            }
        }

        #endregion MakeLockOperation() Tests

        #region MakeUnlockOperation() Tests

        [Test]
        public void MakeUnlockOperation_LandingGearsIsEmpty_CompletesImmediately()
        {
            var testContext = new TestContext();

            testContext.Uut.MakeUnlockOperation()
                .ShouldRunToCompletionIn(1);
        }

        [TestCase(1)]
        [TestCase(10)]
        public void MakeUnlockOperation_LandingGearsIsNotEmpty_UnlocksEachLandingGear(int landingGearCount)
        {
            var testContext = new TestContext();

            var mockLandingGears = Enumerable.Repeat(0, landingGearCount)
                .Select(_ => new Mock<IMyLandingGear>())
                .ToArray();

            foreach (var mockLandingGear in mockLandingGears)
                testContext.Uut.AddLandingGear(mockLandingGear.Object);

            testContext.Uut.MakeUnlockOperation()
                .ShouldRunToCompletionIn(landingGearCount);

            mockLandingGears.ForEach(mockLandingGear =>
                mockLandingGear.ShouldHaveReceived(x => x.Unlock()));
        }

        [TestCase(1)]
        [TestCase(10)]
        public void MakeUnlockOperation_OperationIsDisposed_RecyclesOperation(int landingGearCount)
        {
            var testContext = new TestContext();

            var mockLandingGears = Enumerable.Repeat(0, landingGearCount)
                .Select(_ => new Mock<IMyLandingGear>())
                .ToArray();

            foreach (var mockLandingGear in mockLandingGears)
                testContext.Uut.AddLandingGear(mockLandingGear.Object);

            var result = testContext.Uut.MakeUnlockOperation();
            result.ShouldRunToCompletionIn(landingGearCount);

            var mockLandingGearInvocations = mockLandingGears
                .Select(x => x.Invocations.ToArray())
                .ToArray();

            mockLandingGears.ForEach(x => x
                .Invocations.Clear());

            testContext.Uut.MakeUnlockOperation()
                .ShouldBeSameAs(result);

            result.ShouldRunToCompletionIn(landingGearCount);

            foreach (var i in Enumerable.Range(0, mockLandingGears.Length))
            {
                mockLandingGears[i].Invocations.Count.ShouldBe(mockLandingGearInvocations[i].Length);
                foreach (var j in Enumerable.Range(0, mockLandingGears[i].Invocations.Count))
                    mockLandingGears[i].Invocations[j].ShouldBe(mockLandingGearInvocations[i][j]);
            }
        }

        #endregion MakeUnlockOperation() Tests
    }
}
