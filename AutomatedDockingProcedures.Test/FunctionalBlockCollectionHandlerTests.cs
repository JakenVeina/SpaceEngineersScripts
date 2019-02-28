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
            public int FunctionalBlockCount;

            public TestContext()
            {
                MockFunctionalBlockManager = new Mock<IFunctionalBlockManager>();
                MockFunctionalBlockManager
                    .Setup(x => x.FunctionalBlocks.Count)
                    .Returns(() => FunctionalBlockCount);

                MockLogger = new Mock<ILogger>();

                Uut = new FunctionalBlockCollectionHandler(
                    MockFunctionalBlockManager.Object,
                    MockLogger.Object);

                MockBackgroundWorker = new FakeBackgroundWorker();
            }

            public readonly Mock<IFunctionalBlockManager> MockFunctionalBlockManager;

            public readonly Mock<ILogger> MockLogger;

            public readonly FunctionalBlockCollectionHandler Uut;

            public readonly FakeBackgroundWorker MockBackgroundWorker;

            public Moq.Mock MakeFakeBlock(Type blockType)
                => Activator.CreateInstance(typeof(Mock<>).MakeGenericType(blockType)) as Moq.Mock;
        }

        #endregion Test Context

        #region Test Cases

        public static readonly Type[] BlockTypeTestCases
            = {
                typeof(IMyGyro),
                typeof(IMyLightingBlock),
                typeof(IMyBeacon),
                typeof(IMyRadioAntenna),
                typeof(IMyGasGenerator),
                typeof(IMyReactor),
            };

        #endregion Test Cases

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

        [TestCaseSource(nameof(BlockTypeTestCases))]
        public void MakeCollectBlockOperation_BlockIsRelevantFunctionalBlock_AddsFunctionalBlock(Type blockType)
        {
            var testContext = new TestContext();

            var mockBlock = testContext.MakeFakeBlock(blockType);

            var result = testContext.Uut.MakeCollectBlockOperation(mockBlock.Object as IMyFunctionalBlock);
            result.ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            testContext.MockBackgroundWorker.MockSubOperationScheduler
                .ShouldNotHaveReceived(x => x(It.IsAny<IBackgroundOperation>()));

            testContext.MockFunctionalBlockManager
                .ShouldHaveReceived(x => x.AddFunctionalBlock(mockBlock.Object as IMyFunctionalBlock));

            result.Result.IsSuccess.ShouldBeTrue();
        }

        [TestCaseSource(nameof(BlockTypeTestCases))]
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

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void OnCompleted_Always_LogsFunctionalBlockCount(int functionalBlockCount)
        {
            var testContext = new TestContext()
            {
                FunctionalBlockCount = functionalBlockCount
            };

            testContext.Uut.OnCompleted();

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.Contains(functionalBlockCount.ToString()))));
        }

        #endregion OnCompleted() Tests
    }
}
