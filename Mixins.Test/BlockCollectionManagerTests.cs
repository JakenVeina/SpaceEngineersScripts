using System;
using System.Collections.Generic;
using System.Linq;

using Sandbox.ModAPI;

using Moq;
using NUnit.Framework;
using Shouldly;

using static IngameScript.Program;

using Mixins.Test.Common;

namespace Mixins.Test
{
    [TestFixture]
    public class BlockCollectionManagerTests
    {
        #region Test Context

        private class TestContext
        {
            public TestContext()
            {
                MockGridTerminalSystem = new Mock<IMyGridTerminalSystem>();
                MockGridTerminalSystem
                    .Setup(x => x.GetBlocksOfType(It.IsAny<List<IMyTerminalBlock>>(), It.IsAny<Func<IMyTerminalBlock, bool>>()))
                    .Callback<List<IMyTerminalBlock>, Func<IMyTerminalBlock, bool>>((blocks, collect) =>
                    {
                        blocks.ShouldNotBeNull();

                        blocks.Clear();
                        blocks.AddRange(_mockBlocks.Select(x => x.Object));
                    });

                Uut = new BlockCollectionManager<IMyTerminalBlock>(
                    _blockCollectionHandlers,
                    MockGridTerminalSystem.Object);
            }

            public readonly Mock<IMyGridTerminalSystem> MockGridTerminalSystem;

            public readonly BlockCollectionManager<IMyTerminalBlock> Uut;

            public IReadOnlyList<Mock<IMyTerminalBlock>> MockBlocks
                => _mockBlocks;

            public IReadOnlyList<Mock<IBlockCollectionHandler>> MockBlockCollectionHandlers
                => _mockBlockCollectionHandlers;

            public IReadOnlyList<Mock<IBackgroundOperation<BlockCollectionResult>>> MockCollectBlockOperations
                => _mockCollectBlockOperations;

            public Mock<IMyTerminalBlock> AddMockBlock()
            {
                var mockBlock = new Mock<IMyTerminalBlock>();

                _mockBlocks.Add(mockBlock);

                return mockBlock;
            }

            public Mock<IBlockCollectionHandler> AddMockBlockCollectionHandler(BlockCollectionResult blockCollectionResult)
            {
                var mockBlockCollectionHandler = new Mock<IBlockCollectionHandler>();
                mockBlockCollectionHandler
                    .Setup(x => x.MakeCollectBlockOperation(It.IsAny<IMyTerminalBlock>()))
                    .Returns(() =>
                    {
                        var mockCollectBlockOperation = new Mock<IBackgroundOperation<BlockCollectionResult>>();
                        mockCollectBlockOperation
                            .Setup(x => x.Result)
                            .Returns(blockCollectionResult);

                        mockCollectBlockOperation
                            .Setup(x => x.Execute(It.IsAny<Action<IBackgroundOperation>>()))
                            .Returns(BackgroundOperationResult.Completed);

                        _mockCollectBlockOperations.Add(mockCollectBlockOperation);

                        return mockCollectBlockOperation.Object;
                    });

                _mockBlockCollectionHandlers.Add(mockBlockCollectionHandler);
                _blockCollectionHandlers.Add(mockBlockCollectionHandler.Object);

                return mockBlockCollectionHandler;
            }

            public void ShouldHaveReceivedGetBlocks()
            {
                MockGridTerminalSystem
                    .ShouldHaveReceived(x => x.GetBlocksOfType(It.IsAny<List<IMyTerminalBlock>>(), It.IsAny<Func<IMyTerminalBlock, bool>>()), 1);

                MockGridTerminalSystem
                    .ShouldNotHaveReceived(x => x.GetBlocksOfType(It.IsAny<List<IMyTerminalBlock>>(), It.IsNotNull<Func<IMyTerminalBlock, bool>>()));

                MockGridTerminalSystem
                    .ShouldNotHaveReceived(x => x.GetBlocks(It.IsAny<List<IMyTerminalBlock>>()));
            }

            public void ShouldNotHaveReceivedGetBlocks()
            {
                MockGridTerminalSystem
                    .ShouldNotHaveReceived(x => x.GetBlocksOfType(It.IsAny<List<IMyTerminalBlock>>(), It.IsAny<Func<IMyTerminalBlock, bool>>()));

                MockGridTerminalSystem
                    .ShouldNotHaveReceived(x => x.GetBlocks(It.IsAny<List<IMyTerminalBlock>>()));
            }

            private readonly List<Mock<IMyTerminalBlock>> _mockBlocks
                = new List<Mock<IMyTerminalBlock>>();

            private readonly List<Mock<IBlockCollectionHandler>> _mockBlockCollectionHandlers
                = new List<Mock<IBlockCollectionHandler>>();

            private readonly List<IBlockCollectionHandler> _blockCollectionHandlers
                = new List<IBlockCollectionHandler>();

            private readonly List<Mock<IBackgroundOperation<BlockCollectionResult>>> _mockCollectBlockOperations
                = new List<Mock<IBackgroundOperation<BlockCollectionResult>>>();
        }

        #endregion Test Context

        #region Test Cases

        public static readonly int[][] ValidHandlerAndBlockCounts
            = new[]
            {
                new[] { 1,  1 },
                new[] { 1,  5 },
                new[] { 1,  10 },
                new[] { 5,  1 },
                new[] { 5,  5 },
                new[] { 5,  10 },
                new[] { 10, 1 },
                new[] { 10, 5 },
                new[] { 10, 10 }
            };

        #endregion Test Cases

        #region MakeCollectBlocksOperation() Tests

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        public void MakeCollectBlocksOperation_BlockCollectionHandlersIsEmpty_CompletesImmediately(int blockCount)
        {
            var testContext = new TestContext();

            foreach (var _ in Enumerable.Repeat(0, blockCount))
                testContext.AddMockBlock();

            testContext.Uut.MakeCollectBlocksOperation()
                .ShouldRunToCompletionIn(1);

            testContext.ShouldNotHaveReceivedGetBlocks();
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        public void MakeCollectBlocksOperation_BlocksIsEmpty_CompletesAfterStarting(int handlerCount)
        {
            var testContext = new TestContext();

            foreach (var x in Enumerable.Repeat(0, handlerCount))
                testContext.AddMockBlockCollectionHandler(BlockCollectionResult.Success);

            testContext.Uut.MakeCollectBlocksOperation()
                .ShouldRunToCompletion();

            testContext.ShouldHaveReceivedGetBlocks();

            foreach(var mockBlockCollectionHandler in testContext.MockBlockCollectionHandlers)
            {
                mockBlockCollectionHandler
                    .ShouldHaveReceived(x => x.OnStarting(), 1);
                mockBlockCollectionHandler
                    .ShouldNotHaveReceived(x => x.MakeCollectBlockOperation(It.IsAny<IMyTerminalBlock>()));
                mockBlockCollectionHandler
                    .ShouldHaveReceived(x => x.OnCompleted(), 1);
            }
        }

        [TestCaseSource(nameof(ValidHandlerAndBlockCounts))]
        public void MakeCollectBlocksOperation_BlockCollectionResultsAreSkipped_TerminatesBlockCollectionEarly(int handlerCount, int blockCount)
        {
            var testContext = new TestContext();
            var mockBackgroundWorker = new FakeBackgroundWorker();

            foreach (var _ in Enumerable.Repeat(0, blockCount))
                testContext.AddMockBlock();

            foreach (var _ in Enumerable.Repeat(0, handlerCount))
                testContext.AddMockBlockCollectionHandler(BlockCollectionResult.Skipped);

            testContext.Uut.MakeCollectBlocksOperation()
                .ShouldRunToCompletion(mockBackgroundWorker);

            testContext.ShouldHaveReceivedGetBlocks();

            foreach (var mockBlockCollectionHandler in testContext.MockBlockCollectionHandlers)
            {
                mockBlockCollectionHandler
                    .ShouldHaveReceived(x => x.OnStarting(), 1);
                mockBlockCollectionHandler
                    .ShouldHaveReceived(x => x.OnCompleted(), 1);
            }

            foreach (var mockBlock in testContext.MockBlocks)
            {
                testContext.MockBlockCollectionHandlers.First()
                    .ShouldHaveReceived(x => x.MakeCollectBlockOperation(mockBlock.Object), 1);

                foreach(var mockBlockCollectionHandler in testContext.MockBlockCollectionHandlers.Skip(1))
                    mockBlockCollectionHandler
                        .ShouldNotHaveReceived(x => x.MakeCollectBlockOperation(It.IsAny<IMyTerminalBlock>()));
            }

            testContext.MockCollectBlockOperations.Count.ShouldBe(blockCount);

            foreach (var mockCollectBlockOperation in testContext.MockCollectBlockOperations)
                mockBackgroundWorker.MockSubOperationScheduler
                    .ShouldHaveReceived(x => x(mockCollectBlockOperation.Object));
        }

        [TestCaseSource(nameof(ValidHandlerAndBlockCounts))]
        public void MakeCollectBlocksOperation_BlockCollectionResultsAreSuccess_PerformsAllBlockCollections(int handlerCount, int blockCount)
        {
            var testContext = new TestContext();
            var mockBackgroundWorker = new FakeBackgroundWorker();

            foreach (var _ in Enumerable.Repeat(0, blockCount))
                testContext.AddMockBlock();

            foreach (var _ in Enumerable.Repeat(0, handlerCount))
                testContext.AddMockBlockCollectionHandler(BlockCollectionResult.Success);

            testContext.Uut.MakeCollectBlocksOperation()
                .ShouldRunToCompletion(mockBackgroundWorker);

            testContext.ShouldHaveReceivedGetBlocks();

            foreach (var mockBlockCollectionHandler in testContext.MockBlockCollectionHandlers)
            {
                mockBlockCollectionHandler
                    .ShouldHaveReceived(x => x.OnStarting(), 1);

                foreach (var mockBlock in testContext.MockBlocks)
                    mockBlockCollectionHandler
                        .ShouldHaveReceived(x => x.MakeCollectBlockOperation(mockBlock.Object), 1);

                mockBlockCollectionHandler
                    .ShouldHaveReceived(x => x.OnCompleted(), 1);
            }

            testContext.MockCollectBlockOperations.Count.ShouldBe(handlerCount * blockCount);

            foreach (var mockCollectBlockOperation in testContext.MockCollectBlockOperations)
                mockBackgroundWorker.MockSubOperationScheduler
                    .ShouldHaveReceived(x => x(mockCollectBlockOperation.Object));
        }

        [TestCaseSource(nameof(ValidHandlerAndBlockCounts))]
        public void MakeCollectBlocksOperation_BlockCollectionResultsAreIgnored_PerformsAllBlockCollections(int handlerCount, int blockCount)
        {
            var testContext = new TestContext();
            var mockBackgroundWorker = new FakeBackgroundWorker();

            foreach (var _ in Enumerable.Repeat(0, blockCount))
                testContext.AddMockBlock();

            foreach (var _ in Enumerable.Repeat(0, handlerCount))
                testContext.AddMockBlockCollectionHandler(BlockCollectionResult.Ignored);

            testContext.Uut.MakeCollectBlocksOperation()
                .ShouldRunToCompletion(mockBackgroundWorker);

            testContext.ShouldHaveReceivedGetBlocks();

            foreach (var mockBlockCollectionHandler in testContext.MockBlockCollectionHandlers)
            {
                mockBlockCollectionHandler
                    .ShouldHaveReceived(x => x.OnStarting(), 1);

                foreach (var mockBlock in testContext.MockBlocks)
                    mockBlockCollectionHandler
                        .ShouldHaveReceived(x => x.MakeCollectBlockOperation(mockBlock.Object), 1);

                mockBlockCollectionHandler
                    .ShouldHaveReceived(x => x.OnCompleted(), 1);
            }

            testContext.MockCollectBlockOperations.Count.ShouldBe(handlerCount * blockCount);

            foreach (var mockCollectBlockOperation in testContext.MockCollectBlockOperations)
                mockBackgroundWorker.MockSubOperationScheduler
                    .ShouldHaveReceived(x => x(mockCollectBlockOperation.Object));
        }

        [TestCaseSource(nameof(ValidHandlerAndBlockCounts))]
        public void MakeCollectBlocksOperation_OperationIsDisposed_OperationIsRecycled(int handlerCount, int blockCount)
        {
            var testContext = new TestContext();
            var mockBackgroundWorker = new FakeBackgroundWorker();

            foreach (var _ in Enumerable.Repeat(0, blockCount))
                testContext.AddMockBlock();

            foreach (var _ in Enumerable.Repeat(0, handlerCount))
                testContext.AddMockBlockCollectionHandler(BlockCollectionResult.Ignored);

            var result = testContext.Uut.MakeCollectBlocksOperation();
            result.ShouldRunToCompletion(mockBackgroundWorker);

            var mockBlocksInvocations = testContext.MockBlocks
                .Select(x => x.Invocations.ToArray())
                .ToArray();

            var mockBlockCollectionHandlersInvocations = testContext.MockBlockCollectionHandlers
                .Select(x => x.Invocations.ToArray())
                .ToArray();

            result.ShouldBeAssignableTo<IDisposable>();
            (result as IDisposable).Dispose();

            testContext.MockBlocks
                .ForEach(x => x.Invocations.Clear());
            testContext.MockBlockCollectionHandlers
                .ForEach(x => x.Invocations.Clear());

            testContext.Uut.MakeCollectBlocksOperation()
                .ShouldBeSameAs(result);

            result.ShouldRunToCompletion(mockBackgroundWorker);

            foreach (var i in Enumerable.Range(0, mockBlocksInvocations.Length))
            {
                testContext.MockBlocks[i].Invocations.Count.ShouldBe(mockBlocksInvocations[i].Length);
                foreach (var j in Enumerable.Range(0, mockBlocksInvocations[i].Length))
                    testContext.MockBlocks[i].Invocations[j].ShouldBe(mockBlocksInvocations[i][j]);
            }

            foreach (var i in Enumerable.Range(0, mockBlockCollectionHandlersInvocations.Length))
            {
                testContext.MockBlockCollectionHandlers[i].Invocations.Count.ShouldBe(mockBlockCollectionHandlersInvocations[i].Length);
                foreach (var j in Enumerable.Range(0, mockBlockCollectionHandlersInvocations[i].Length))
                    testContext.MockBlockCollectionHandlers[i].Invocations[j].ShouldBe(mockBlockCollectionHandlersInvocations[i][j]);
            }
        }

        #endregion MakeCollectBlocksOperation() Tests
    }
}
