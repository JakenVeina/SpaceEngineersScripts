using System;
using System.Collections.Generic;

using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    public partial class Program
    {
        public interface IBackgroundOperation
        {
            BackgroundOperationResult Execute(Action<IBackgroundOperation> subOperationScheduler);
        }

        public interface IBackgroundOperation<TResult> : IBackgroundOperation
        {
            TResult Result { get; }
        }

        public interface IBackgroundWorker
        {
            void RegisterRecurringOperation(TimeSpan interval, Func<IBackgroundOperation> operationConstructor);

            void ClearRecurringOperations();

            void ScheduleOperation(IBackgroundOperation operation);

            void ExecuteOperations();
        }

        public partial class BackgroundWorker : IBackgroundWorker
        {
            private static readonly TimeSpan IdealTickInterval
                = TimeSpan.FromMilliseconds(17);

            private static readonly TimeSpan Ideal10TickInterval
                = TimeSpan.FromMilliseconds(167);

            private static readonly TimeSpan Ideal100TickInterval
                = TimeSpan.FromMilliseconds(1667);

            public BackgroundWorker(
                IBackgroundWorkerSettingsProvider backgroundWorkerSettingsProvider,
                IDateTimeProvider dateTimeProvider,
                IMyGridProgramRuntimeInfo gridProgramRuntimeInfo)
            {
                _backgroundWorkerSettingsProvider = backgroundWorkerSettingsProvider;
                _dateTimeProvider = dateTimeProvider;
                _gridProgramRuntimeInfo = gridProgramRuntimeInfo;

                _subOperationScheduler = _scheduledSubOperations.Push;
            }

            public void RegisterRecurringOperation(TimeSpan interval, Func<IBackgroundOperation> operationConstructor)
                => _recurringOperations.Add(new RecurringOperation()
                {
                    OperationConstructor = operationConstructor,
                    Interval = interval,
                    NextOccurrance = _dateTimeProvider.Now + interval
                });

            public void ClearRecurringOperations()
                => _recurringOperations.Clear();

            public void ScheduleOperation(IBackgroundOperation operation)
                => _scheduledOperations.Enqueue(operation);

            public void ExecuteOperations()
            {
                if (_scheduledOperations.Count == 0)
                {
                    ScheduleRecurringOperations();
                    return;
                }

                while (_gridProgramRuntimeInfo.CurrentInstructionCount < _backgroundWorkerSettingsProvider.Settings.InstructionsPerExecution)
                {
                    if (_scheduledSubOperations.Count > 0)
                    {
                        if (_scheduledSubOperations.Peek().Execute(_subOperationScheduler).IsComplete)
                        {
                            var disposable = _scheduledSubOperations.Pop() as IDisposable;
                            if (disposable != null)
                                disposable.Dispose();
                        }
                    }
                    else if (_scheduledOperations.Count > 0)
                    {
                        if (_scheduledOperations.Peek().Execute(_subOperationScheduler).IsComplete)
                        {
                            var disposable = _scheduledOperations.Dequeue() as IDisposable;
                            if (disposable != null)
                                disposable.Dispose();
                        }
                    }
                    else
                        break;
                }

                _gridProgramRuntimeInfo.UpdateFrequency |= UpdateFrequency.Once;
            }

            internal protected IReadOnlyList<RecurringOperation> RecurringOperations
                => _recurringOperations;

            internal protected IReadOnlyCollection<IBackgroundOperation> ScheduledOperations
                => _scheduledOperations;

            internal protected IReadOnlyCollection<IBackgroundOperation> ScheduledSubOperations
                => _scheduledSubOperations;

            private void ScheduleRecurringOperations()
            {
                if(RecurringOperations.Count == 0)
                {
                    _gridProgramRuntimeInfo.UpdateFrequency = UpdateFrequency.None;
                    return;
                }

                var now = _dateTimeProvider.Now;

                var nextRecurringOperation = DateTime.MaxValue;

                for (int i = 0; i < RecurringOperations.Count; ++i)
                {
                    var recurringOperation = RecurringOperations[i];

                    if (recurringOperation.NextOccurrance < nextRecurringOperation)
                        nextRecurringOperation = recurringOperation.NextOccurrance;

                    if (recurringOperation.NextOccurrance <= now)
                    {
                        _scheduledOperations.Enqueue(recurringOperation.OperationConstructor.Invoke());
                        recurringOperation.NextOccurrance = _dateTimeProvider.Now + recurringOperation.Interval;
                        _recurringOperations[i] = recurringOperation;
                    }
                }

                var nextOperationInterval = nextRecurringOperation - now;

                _gridProgramRuntimeInfo.UpdateFrequency |= (nextOperationInterval >= Ideal100TickInterval) ? UpdateFrequency.Update100
                    : (nextOperationInterval >= Ideal10TickInterval) ? UpdateFrequency.Update10
                    : UpdateFrequency.Once;
            }

            private readonly IBackgroundWorkerSettingsProvider _backgroundWorkerSettingsProvider;

            private readonly IDateTimeProvider _dateTimeProvider;

            private readonly IMyGridProgramRuntimeInfo _gridProgramRuntimeInfo;

            private readonly List<RecurringOperation> _recurringOperations
                = new List<RecurringOperation>();

            private readonly Queue<IBackgroundOperation> _scheduledOperations
                = new Queue<IBackgroundOperation>();

            private readonly Stack<IBackgroundOperation> _scheduledSubOperations
                = new Stack<IBackgroundOperation>();

            private readonly Action<IBackgroundOperation> _subOperationScheduler;

            internal protected struct RecurringOperation
            {
                public Func<IBackgroundOperation> OperationConstructor;

                public TimeSpan Interval;

                public DateTime NextOccurrance;
            }
        }
    }
}
