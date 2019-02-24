using System;
using System.Linq;

using Sandbox.ModAPI.Ingame;

using Moq;
using NUnit.Framework;
using Shouldly;

using static IngameScript.Program;

using Mixins.Test.Common;

namespace DoorManager.Test
{
    [TestFixture]
    public class ManagedDoorCollectionHandlerTests
    {
        #region Test Context

        private class TestContext
        {
            public TimeSpan AutoCloseInterval;

            public int DoorCount;

            public TestContext()
            {
                MockDoorManager = new Mock<IDoorManager>();
                MockDoorManager
                    .Setup(x => x.DoorCount)
                    .Returns(() => DoorCount);

                MockLogger = new Mock<ILogger>();

                MockManagedDoorSettingsProvider = new Mock<IManagedDoorSettingsProvider>();
                MockManagedDoorSettingsProvider
                    .Setup(x => x.Settings)
                    .Returns(() => new ManagedDoorSettings()
                    {
                        AutoCloseInterval = AutoCloseInterval
                    });

                Uut = new ManagedDoorCollectionHandler(
                    MockDoorManager.Object,
                    MockLogger.Object,
                    MockManagedDoorSettingsProvider.Object);

                MockBackgroundWorker = new FakeBackgroundWorker();
            }

            public readonly Mock<IDoorManager> MockDoorManager;

            public readonly Mock<ILogger> MockLogger;

            public readonly Mock<IManagedDoorSettingsProvider> MockManagedDoorSettingsProvider;

            public readonly ManagedDoorCollectionHandler Uut;

            public readonly FakeBackgroundWorker MockBackgroundWorker;
        }

        #endregion Test Context

        #region OnStarting() Tests

        [Test]
        public void OnStarting_Always_ClearsDoors()
        {
            var testContext = new TestContext();

            testContext.Uut.OnStarting();

            testContext.MockDoorManager
                .ShouldHaveReceived(x => x.ClearDoors());
        }

        #endregion OnStarting() Tests

        #region MakeCollectBlockOperation() Tests

        [Test]
        public void MakeCollectBlockOperation_BlockIsNotDoor_IgnoresBlock()
        {
            var testContext = new TestContext();

            var mockBlock = new Mock<IMyTerminalBlock>();

            var result = testContext.Uut.MakeCollectBlockOperation(mockBlock.Object);

            result
                .ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            testContext.MockBackgroundWorker.MockSubOperationScheduler
                .ShouldNotHaveReceived(x => x(It.IsAny<IBackgroundOperation>()));

            testContext.MockDoorManager
                .ShouldNotHaveReceived(x => x.AddDoor(It.IsAny<IMyDoor>(), It.IsAny<ManagedDoorSettings>()));

            result.Result.IsIgnored.ShouldBeTrue();
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        public void MakeCollectBlockOperation_BlockIsDoor_AddsManagedDoor(int autoCloseIntervalMilliseconds)
        {
            var testContext = new TestContext()
            {
                AutoCloseInterval = TimeSpan.FromMilliseconds(autoCloseIntervalMilliseconds)
            };

            var mockBlock = new Mock<IMyDoor>();

            var result = testContext.Uut.MakeCollectBlockOperation(mockBlock.Object);

            result
                .ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            testContext.MockBackgroundWorker.MockSubOperationScheduler
                .ShouldNotHaveReceived(x => x(It.IsAny<IBackgroundOperation>()));

            testContext.MockDoorManager
                .ShouldHaveReceived(x => x.AddDoor(mockBlock.Object, testContext.MockManagedDoorSettingsProvider.Object.Settings), 1);

            result.Result.IsSuccess.ShouldBeTrue();
        }

        [Test]
        public void MakeCollectBlockOperation_OperationIsDisposed_RecyclesOperation()
        {
            var testContext = new TestContext();

            var mockBlock = new Mock<IMyDoor>();

            var result = testContext.Uut.MakeCollectBlockOperation(mockBlock.Object);
            result.ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            var mockBlockInvocations = mockBlock
                .Invocations.ToArray();

            var mockDoorManagerInvocations = testContext.MockDoorManager
                .Invocations.ToArray();

            result.ShouldBeAssignableTo<IDisposable>();
            (result as IDisposable).Dispose();

            mockBlock
                .Invocations.Clear();

            testContext.MockDoorManager
                .Invocations.Clear();

            var secondMockBlock = new Mock<IMyDoor>();

            testContext.Uut.MakeCollectBlockOperation(secondMockBlock.Object)
                .ShouldBeSameAs(result);

            result.ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            secondMockBlock.Invocations.Count.ShouldBe(mockBlockInvocations.Length);
            foreach (var i in Enumerable.Range(0, mockBlockInvocations.Length))
                secondMockBlock.Invocations[i].ShouldBe(mockBlockInvocations[i]);

            testContext.MockDoorManager.Invocations.Count.ShouldBe(mockDoorManagerInvocations.Length);
            foreach (var i in Enumerable.Range(0, mockDoorManagerInvocations.Length))
                testContext.MockDoorManager.Invocations[i].ShouldBe(mockDoorManagerInvocations[i]);
        }

        #endregion MakeCollectBlockOperation() Tests

        #region OnCompleted() Tests

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void OnCompleted_Always_LogsDoorCount(int doorCount)
        {
            var testContext = new TestContext()
            {
                DoorCount = doorCount
            };

            testContext.Uut.OnCompleted();

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.Contains(doorCount.ToString()))));
        }

        #endregion OnCompleted() Tests
    }
}
