using System;
using System.Collections.Generic;
using System.Linq;

using Sandbox.ModAPI.Ingame;

using Moq;
using NUnit.Framework;
using Shouldly;

using static IngameScript.Program;

namespace Mixins.Test
{
    [TestFixture]
    public class BackgroundWorkerTests
    {
        #region Test Context

        private class TestContext
        {
            public TestContext()
            {
                MockBackgroundWorkerSettingsProvider = new Mock<IBackgroundWorkerSettingsProvider>();
                MockBackgroundWorkerSettingsProvider
                    .Setup(x => x.Settings)
                    .Returns(() => new BackgroundWorkerSettings()
                    {
                        InstructionsPerExecution = InstructionsPerExecution
                    });

                MockDateTimeProvider = new Mock<IDateTimeProvider>();
                MockDateTimeProvider
                    .Setup(x => x.Now)
                    .Returns(() => Now);

                MockGridProgramRuntimeInfo = new Mock<IMyGridProgramRuntimeInfo>();
                MockGridProgramRuntimeInfo
                    .Setup(x => x.CurrentInstructionCount)
                    .Returns(() => CurrentInstructionCount);

                Uut = new BackgroundWorker(
                    MockBackgroundWorkerSettingsProvider.Object,
                    MockDateTimeProvider.Object,
                    MockGridProgramRuntimeInfo.Object);
            }

            public DateTime Now
                = DateTime.Now;

            public int CurrentInstructionCount
                = 0;

            public int InstructionsPerExecution
                = 1000;

            public Mock<IBackgroundWorkerSettingsProvider> MockBackgroundWorkerSettingsProvider;

            public Mock<IDateTimeProvider> MockDateTimeProvider;

            public Mock<IMyGridProgramRuntimeInfo> MockGridProgramRuntimeInfo;

            public BackgroundWorker Uut;

            public Mock<IBackgroundOperation> MakeFakeBackgroundOperation(int executeCount)
            {
                var mockBackgroundOperation = new Mock<IBackgroundOperation>();

                var executeCounter = 0;

                mockBackgroundOperation
                    .Setup(x => x.Execute(It.IsAny<Action<IBackgroundOperation>>()))
                    .Returns(() =>
                    {
                        ++CurrentInstructionCount;

                        return (++executeCounter >= executeCount)
                            ? BackgroundOperationResult.Completed
                            : BackgroundOperationResult.NotCompleted;
                    });

                return mockBackgroundOperation;
            }

            public Mock<IBackgroundOperation> MakeFakeDisposableBackgroundOperation(int executeCount)
            {
                var mockBackgroundOperation = new Mock<IBackgroundOperation>();

                var executeCounter = 0;

                mockBackgroundOperation
                    .As<IDisposable>();

                mockBackgroundOperation
                    .Setup(x => x.Execute(It.IsAny<Action<IBackgroundOperation>>()))
                    .Returns(() =>
                    {
                        ++CurrentInstructionCount;

                        mockBackgroundOperation
                            .As<IDisposable>()
                            .ShouldNotHaveReceived(x => x.Dispose());

                        return (++executeCounter >= executeCount)
                            ? BackgroundOperationResult.Completed
                            : BackgroundOperationResult.NotCompleted;
                    });

                return mockBackgroundOperation;
            }

            public Mock<Func<IBackgroundOperation>> MakeFakeRecurringOperationConstructor(Action<Mock<IBackgroundOperation>> onInvoked, int operationExecuteCount)
            {
                var mockRecurringOperationConstructor = new Mock<Func<IBackgroundOperation>>();

                mockRecurringOperationConstructor
                    .Setup(y => y())
                    .Returns(() =>
                    {
                        var mockOperation = MakeFakeBackgroundOperation(2);

                        onInvoked?.Invoke(mockOperation);

                        return mockOperation.Object;
                    });

                return mockRecurringOperationConstructor;
            }
        }

        #endregion Test Context

        #region Test Cases

        private static readonly int[][] OperationSimulationTestCases
            = (new[] { 1, 3, 10 })
                .SelectMany(x => (new[] { 1, 3, 5 }),
                    (operationCount, instructionsPerOperation) => new { operationCount, instructionsPerOperation })
                .SelectMany(x => (new[] { 1, 5, 10 }),
                    (x, instructionsPerExecution) => (new[] { x.operationCount, x.instructionsPerOperation, instructionsPerExecution }))
                .ToArray();

        #endregion Test Cases

        #region RegisterRecurringOperation() Tests

        [TestCase(1)]
        [TestCase(100)]
        [TestCase(1000)]
        public void RegisterRecurringOperation_Always_Succeeds(int intervalMilliseconds)
        {
            var testContext = new TestContext();

            var mockOperationConstructor = new Mock<Func<IBackgroundOperation>>();

            var interval = TimeSpan.FromMilliseconds(intervalMilliseconds);

            testContext.Uut.RegisterRecurringOperation(interval, mockOperationConstructor.Object);

            testContext.Uut.RecurringOperations.Count.ShouldBe(1);

            var recurringOperation = testContext.Uut.RecurringOperations.Single();
            recurringOperation.OperationConstructor.ShouldBeSameAs(mockOperationConstructor.Object);
            recurringOperation.Interval.ShouldBe(interval);
            recurringOperation.NextOccurrance.ShouldBe(testContext.MockDateTimeProvider.Object.Now + interval);
        }

        #endregion RegisterRecurringOperation() Tests

        #region ClearRecurringOperations() Tests

        [TestCase(1)]
        [TestCase(100)]
        [TestCase(1000)]
        public void ClearRecurringOperations_Always_Succeeds(int intervalMilliseconds)
        {
            var testContext = new TestContext();

            var mockOperationConstructor = new Mock<Func<IBackgroundOperation>>();

            var interval = TimeSpan.FromMilliseconds(intervalMilliseconds);

            testContext.Uut.RegisterRecurringOperation(interval, mockOperationConstructor.Object);

            testContext.Uut.ClearRecurringOperations();

            testContext.Uut.RecurringOperations.ShouldBeEmpty();
        }

        #endregion ClearRecurringOperations() Tests

        #region ScheduleOperation() Tests

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void ScheduleOperation_Always_EnqueuesOperation(int operationCount)
        {
            var testContext = new TestContext();

            foreach (var x in Enumerable.Repeat<Func<int, Mock<IBackgroundOperation>>>(testContext.MakeFakeBackgroundOperation, operationCount))
                testContext.Uut.ScheduleOperation(x.Invoke(1).Object);

            var mockOperation = testContext.MakeFakeBackgroundOperation(1);

            testContext.Uut.ScheduleOperation(mockOperation.Object);

            testContext.Uut.ScheduledOperations.Count.ShouldBe(operationCount + 1);
            testContext.Uut.ScheduledOperations.Last().ShouldBeSameAs(mockOperation.Object);

            mockOperation.ShouldNotHaveReceived(x => x.Execute(It.IsAny<Action<IBackgroundOperation>>()));
        }

        #endregion ScheduleOperation() Tests

        #region ExecuteOperations() Tests

        public void ExecuteOperations_RecurringOperationsIsEmpty_CancelsUpdates()
        {
            var testContext = new TestContext();

            testContext.Uut.ExecuteOperations();

            testContext.MockGridProgramRuntimeInfo.ShouldHaveReceivedSet(x => x.UpdateFrequency = UpdateFrequency.None);

            testContext.Uut.ScheduledOperations.ShouldBeEmpty();
        }

        [TestCase(0,  1)]
        [TestCase(0,  10)]
        [TestCase(1,  1)]
        [TestCase(1,  10)]
        [TestCase(10, 1)]
        [TestCase(10, 10)]
        public void ExecuteOperations_RecurringOperationsHaveMatured_SchedulesRecurringOperations(
            int prematureRecurringOperationCount,
            int matureRecurringOperationCount)
        {
            var testContext = new TestContext();

            var mockPrematureRecurringOperationConstructors = Enumerable.Repeat(0, prematureRecurringOperationCount)
                .Select(x => testContext.MakeFakeRecurringOperationConstructor(null, 2))
                .ToArray();
            var prematureInterval = TimeSpan.FromMinutes(2);
            foreach (var mockPrematureRecurringOperationConstructor in mockPrematureRecurringOperationConstructors)
                testContext.Uut.RegisterRecurringOperation(prematureInterval, mockPrematureRecurringOperationConstructor.Object);

            var mockMatureRecurringOperations = new List<Mock<IBackgroundOperation>>();

            var mockMatureRecurringOperationConstructors = Enumerable.Repeat(0, matureRecurringOperationCount)
                .Select(x => testContext.MakeFakeRecurringOperationConstructor(mockMatureRecurringOperations.Add, 2))
                .ToArray();
            var matureInterval = TimeSpan.FromMinutes(1);
            foreach (var mockMatureRecurringOperationConstructor in mockMatureRecurringOperationConstructors)
                testContext.Uut.RegisterRecurringOperation(matureInterval, mockMatureRecurringOperationConstructor.Object);

            testContext.Now += matureInterval;

            testContext.MockGridProgramRuntimeInfo.Invocations.Clear();

            testContext.Uut.ExecuteOperations();

            testContext.Uut.ScheduledOperations.ShouldBe(
                mockMatureRecurringOperations.Select(x => x.Object),
                ignoreOrder: true);

            testContext.MockGridProgramRuntimeInfo.ShouldHaveReceivedSet(x => x.UpdateFrequency = It.Is<UpdateFrequency>(y => y.HasFlag(UpdateFrequency.Once)));

            mockPrematureRecurringOperationConstructors.ForEach(prematureRecurringOperationConstructor =>
                prematureRecurringOperationConstructor.ShouldNotHaveReceived(x => x()));

            mockMatureRecurringOperationConstructors.ForEach(mockMatureRecurringOperationConstructor =>
                mockMatureRecurringOperationConstructor.ShouldHaveReceived(x => x(), 1));

            mockMatureRecurringOperations.ForEach(mockMatureRecurringOperation =>
                mockMatureRecurringOperation.ShouldNotHaveReceived(x => x.Execute(It.IsAny<Action<IBackgroundOperation>>())));
        }

        [TestCase(0,  0,    UpdateFrequency.Update100)]
        [TestCase(1,  1,    UpdateFrequency.Once)]
        [TestCase(1,  16,   UpdateFrequency.Once)]
        [TestCase(1,  17,   UpdateFrequency.Once)]
        [TestCase(1,  166,  UpdateFrequency.Once)]
        [TestCase(1,  167,  UpdateFrequency.Update10)]
        [TestCase(1,  1666, UpdateFrequency.Update10)]
        [TestCase(1,  1667, UpdateFrequency.Update100)]
        public void ExecuteOperations_NoRecurringOperationsHaveMatured_SetsUpdateFrequency(
            int recurringOperationCount,
            int intervalMilliseconds,
            UpdateFrequency expectedUpdateFrequency)
        {
            var testContext = new TestContext();

            var mockRecurringOperationConstructors = Enumerable.Repeat(0, recurringOperationCount)
                .Select(x => testContext.MakeFakeRecurringOperationConstructor(null, 2))
                .ToArray();
            var interval = TimeSpan.FromMilliseconds(intervalMilliseconds);
            foreach (var mockRecurringOperationConstructor in mockRecurringOperationConstructors)
                testContext.Uut.RegisterRecurringOperation(interval, mockRecurringOperationConstructor.Object);

            testContext.Uut.ExecuteOperations();

            testContext.MockGridProgramRuntimeInfo.ShouldHaveReceivedSet(x => x.UpdateFrequency = It.Is<UpdateFrequency>(y => y.HasFlag(expectedUpdateFrequency)));

            testContext.Uut.ScheduledOperations.ShouldBeEmpty();

            mockRecurringOperationConstructors.ForEach(mockRecurringOperation =>
                mockRecurringOperation.ShouldNotHaveReceived(x => x()));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void ExecuteOperations_SubOperationSchedulerIsInvoked_SchedulesSubOperation(int existingOperationCount)
        {
            var testContext = new TestContext();

            var mockOperations = Enumerable.Repeat<Func<int, Mock<IBackgroundOperation>>>(testContext.MakeFakeBackgroundOperation, existingOperationCount)
                .Select(x => x.Invoke(1))
                .Prepend(new Mock<IBackgroundOperation>())
                .ToArray();

            var mockSubOperation = testContext.MakeFakeBackgroundOperation(1);

            var hasSubOperationBeenScheduled = false;
            mockOperations[0]
                .Setup(x => x.Execute(It.IsAny<Action<IBackgroundOperation>>()))
                .Returns<Action<IBackgroundOperation>>(subOperationScheduler =>
                {
                    if (!hasSubOperationBeenScheduled)
                    {
                        subOperationScheduler.Invoke(mockSubOperation.Object);

                        mockSubOperation.ShouldNotHaveReceived(x => x.Execute(It.IsAny<Action<IBackgroundOperation>>()));

                        testContext.Uut.ScheduledSubOperations.Count.ShouldBe(1);
                        testContext.Uut.ScheduledSubOperations.First().ShouldBeSameAs(mockSubOperation.Object);

                        mockSubOperation.Invocations.Clear();

                        hasSubOperationBeenScheduled = true;
                        return BackgroundOperationResult.NotCompleted;
                    }

                    return BackgroundOperationResult.Completed;
                });

            foreach(var mockOperation in mockOperations)
                testContext.Uut.ScheduleOperation(mockOperation.Object);

            testContext.Uut.ExecuteOperations();

            mockSubOperation.ShouldHaveReceived(x => x.Execute(It.IsAny<Action<IBackgroundOperation>>()), 1);
        }

        [TestCaseSource(nameof(OperationSimulationTestCases))]
        public void ExecuteOperations_ScheduledOperationsHaveNotCompleted_ExecutesOperations(int operationCount, int instructionsPerOperation, int instructionsPerExecution)
        {
            var testContext = new TestContext()
            {
                InstructionsPerExecution = instructionsPerExecution
            };

            var mockOperations = Enumerable.Range(1, operationCount)
                .Select(x => testContext.MakeFakeBackgroundOperation(instructionsPerOperation))
                .ToArray();
            foreach (var mockOperation in mockOperations)
            {
                testContext.Uut.ScheduleOperation(mockOperation.Object);
                mockOperation.Invocations.Clear();
            }

            var executionsNeeded = (int)Math.Ceiling((float)(operationCount * instructionsPerOperation) / instructionsPerExecution);
            var remainingInstructions = (operationCount * instructionsPerOperation) % instructionsPerExecution;
            foreach (var completedExecutionCount in Enumerable.Range(1, executionsNeeded))
            {
                testContext.Uut.ExecuteOperations();

                var completedOperationCount = (int)Math.Floor((float)(instructionsPerExecution * completedExecutionCount) / instructionsPerOperation);

                testContext.Uut.ScheduledOperations.ShouldBe(
                    mockOperations
                        .Select(x => x.Object)
                        .Skip(completedOperationCount)
                        .ToArray(),
                    ignoreOrder: false);

                testContext.MockGridProgramRuntimeInfo.ShouldHaveReceivedSet(x => x.UpdateFrequency = It.Is<UpdateFrequency>(y => y.HasFlag(UpdateFrequency.Once)));
                testContext.MockGridProgramRuntimeInfo.Invocations.Clear();

                testContext.CurrentInstructionCount.ShouldBe(((completedExecutionCount == executionsNeeded) && (remainingInstructions != 0))
                    ? remainingInstructions
                    : instructionsPerExecution);

                testContext.CurrentInstructionCount = 0;
            }

            foreach(var mockOperation in mockOperations)
                mockOperation.ShouldHaveReceived(x => x.Execute(It.IsAny<Action<IBackgroundOperation>>()), instructionsPerOperation);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(10)]
        public void ExecuteOperations_OperationIsDisposable_InvokesDisposeWhenComplete(int executeCount)
        {
            var testContext = new TestContext();

            var mockOperation = testContext.MakeFakeDisposableBackgroundOperation(executeCount);
            
            testContext.Uut.ScheduleOperation(mockOperation.Object);

            testContext.Uut.ExecuteOperations();

            mockOperation
                .As<IDisposable>()
                .ShouldHaveReceived(x => x.Dispose(), 1);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(10)]
        public void ExecuteOperations_SubOperationIsDisposable_InvokesDisposeWhenComplete(int executeCount)
        {
            var testContext = new TestContext();

            var mockSubOperation = testContext.MakeFakeDisposableBackgroundOperation(executeCount);

            var mockOperation = new Mock<IBackgroundOperation>();
            mockOperation
                .Setup(x => x.Execute(It.IsAny<Action<IBackgroundOperation>>()))
                .Returns<Action<IBackgroundOperation>>(subOperationScheduler =>
                {
                    subOperationScheduler.Invoke(mockSubOperation.Object);

                    return BackgroundOperationResult.Completed;
                });

            testContext.Uut.ScheduleOperation(mockOperation.Object);

            testContext.Uut.ExecuteOperations();

            mockSubOperation
                .As<IDisposable>()
                .ShouldHaveReceived(x => x.Dispose(), 1);
        }

        #endregion ExecuteOperations() Tests
    }
}
