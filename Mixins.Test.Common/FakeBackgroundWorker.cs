using System;
using System.Collections.Generic;

using Moq;
using Shouldly;

using static IngameScript.Program;

namespace Mixins.Test.Common
{
    public class FakeBackgroundWorker
        : Mock<IBackgroundWorker>
    {
        public FakeBackgroundWorker()
        {
            MockSubOperationScheduler = new Mock<Action<IBackgroundOperation>>();
            MockSubOperationScheduler
                .Setup(x => x(It.IsAny<IBackgroundOperation>()))
                .Callback<IBackgroundOperation>(subOperation =>
                {
                    _scheduledOperations.AddToFront(subOperation);
                });

            Setup(x => x.ScheduleOperation(It.IsAny<IBackgroundOperation>()))
                .Callback<IBackgroundOperation>(operation =>
                {
                    operation.ShouldNotBeNull();
                    _scheduledOperations.AddToBack(operation);
                });
        }

        public int MaxExecuteCount { get; set; }
            = 100000;

        public int ExecuteCount { get; private set; }

        public Mock<Action<IBackgroundOperation>> MockSubOperationScheduler { get; }

        public IReadOnlyList<IBackgroundOperation> ScheduledOperations
            => _scheduledOperations;
        private readonly Deque<IBackgroundOperation> _scheduledOperations
            = new Deque<IBackgroundOperation>();

        public void ShouldCompleteOperation(IBackgroundOperation backgroundOperation)
        {
            _scheduledOperations.AddToBack(backgroundOperation);

            ExecuteCount = 0;
            while (ExecuteCount < MaxExecuteCount)
            {
                ++ExecuteCount;
                if (_scheduledOperations[0].Execute(MockSubOperationScheduler.Object).IsComplete)
                {
                    var disposableOperation = _scheduledOperations.RemoveFromFront() as IDisposable;
                    if (disposableOperation != null)
                        disposableOperation.Dispose();

                    if (_scheduledOperations.Count == 0)
                        return;
                }
            }

            throw new ShouldAssertException($"Operation of type {backgroundOperation.GetType().Name} should have completed within {MaxExecuteCount} executions, but didn't");
        }

        public void ShouldCompleteOperationIn(IBackgroundOperation backgroundOperation, int expectedExecuteCount)
        {
            ShouldCompleteOperation(backgroundOperation);

            if (ExecuteCount != expectedExecuteCount)
                throw new ShouldAssertException($"Operation of type {backgroundOperation.GetType().Name} should have completed in {expectedExecuteCount} executions, but instead completed in {ExecuteCount}");
        }

        public void ShouldCompleteOperations()
        {
            ExecuteCount = 0;
            while (ExecuteCount < MaxExecuteCount)
            {
                ++ExecuteCount;
                if (_scheduledOperations[0].Execute(MockSubOperationScheduler.Object).IsComplete)
                {
                    _scheduledOperations.RemoveFromFront();

                    if (_scheduledOperations.Count == 0)
                        return;
                }
            }

            throw new ShouldAssertException($"All operations should have completed within {MaxExecuteCount} executions, but didn't");
        }

        public void ShouldCompleteOperationsIn(int expectedExecuteCount)
        {
            ShouldCompleteOperations();

            if (ExecuteCount != expectedExecuteCount)
                throw new ShouldAssertException($"All operations should have completed in {expectedExecuteCount} executions, but instead completed in {ExecuteCount}");
        }
    }
}
