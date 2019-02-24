using System;
using System.Linq;

using Sandbox.ModAPI.Ingame;

using Moq;
using NUnit.Framework;
using Shouldly;

using static IngameScript.Program;

namespace DoorManager.Test
{
    [TestFixture]
    public class DoorManagerTests
    {
        #region Test Context

        private class TestContext
        {
            public TestContext()
            {
                Now = DateTime.Now;
                AutoCloseInterval = TimeSpan.FromSeconds(1);

                MockDateTimeProvider = new Mock<IDateTimeProvider>();
                MockDateTimeProvider
                    .Setup(x => x.Now)
                    .Returns(() => Now);

                MockDoorManagerSettingsProvider = new Mock<IDoorManagerSettingsProvider>();
                MockDoorManagerSettingsProvider
                    .Setup(x => x.Settings)
                    .Returns(() => new DoorManagerSettings()
                    {
                        AutoCloseInterval = AutoCloseInterval
                    });

                MockLogger = new Mock<ILogger>();

                Uut = new IngameScript.Program.DoorManager(
                    MockDateTimeProvider.Object,
                    MockDoorManagerSettingsProvider.Object,
                    MockLogger.Object);
            }

            public DateTime Now;

            public TimeSpan AutoCloseInterval;

            public Mock<IDateTimeProvider> MockDateTimeProvider;

            public Mock<IDoorManagerSettingsProvider> MockDoorManagerSettingsProvider;

            public Mock<ILogger> MockLogger;

            public IngameScript.Program.DoorManager Uut;

            public Mock<IMyDoor> MakeFakeDoor(DoorStatus status)
            {
                var mockDoor = new Mock<IMyDoor>();

                mockDoor
                    .Setup(x => x.Status)
                    .Returns(() => status);

                mockDoor
                    .Setup(x => x.CloseDoor())
                    .Callback(() =>
                    {
                        if (status != DoorStatus.Closed)
                            status = DoorStatus.Closing;
                    });

                return mockDoor;
            }
        }

        #endregion Test Context

        #region DoorCount Tests

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void DoorCount_Always_ReturnsManagedDoorsCount(int doorCount)
        {
            var testContext = new TestContext();

            var mockDoors = Enumerable.Repeat(0, doorCount)
                .Select(x => testContext.MakeFakeDoor(DoorStatus.Closed))
                .ToArray();
            foreach (var mockDoor in mockDoors)
                testContext.Uut.AddDoor(mockDoor.Object, new ManagedDoorSettings());

            testContext.Uut.DoorCount.ShouldBe(doorCount);
        }

        #endregion DoorCount Tests

        #region IsLockdownEnabled Tests

        [TestCase(false)]
        [TestCase(true)]
        public void IsLockdownEnabled_Always_SavesValue(bool isLockdownEnabled)
        {
            var testContext = new TestContext();

            testContext.Uut.IsLockdownEnabled = isLockdownEnabled;

            testContext.Uut.IsLockdownEnabled.ShouldBe(isLockdownEnabled);
        }

        [TestCase(false, true )]
        [TestCase(true,  false)]
        public void IsLockdownEnabled_ValueChanges_LogsChange(bool oldValue, bool newValue)
        {
            var testContext = new TestContext();

            testContext.Uut.IsLockdownEnabled = oldValue;

            testContext.MockLogger
                .Invocations.Clear();

            testContext.Uut.IsLockdownEnabled = newValue;

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.Contains("Lockdown"))));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void IsLockdownEnabled_ValueDoesNotChange_DoesNotLogChange(bool isLockdownEnabled)
        {
            var testContext = new TestContext();

            testContext.Uut.IsLockdownEnabled = isLockdownEnabled;

            testContext.MockLogger
                .Invocations.Clear();

            testContext.Uut.IsLockdownEnabled = isLockdownEnabled;

            testContext.MockLogger
                .ShouldNotHaveReceived(x => x.AddLine(It.IsAny<string>()));
        }

        #endregion IsLockdownEnabled Tests

        #region AddDoor() Tests

        [TestCase(1,  1)]
        [TestCase(10, 1)]
        [TestCase(1,  10)]
        [TestCase(10, 10)]
        [TestCase(1,  100)]
        [TestCase(10, 100)]
        public void AddDoor_Always_AddsToManagedDoors(int doorCount, int autoCloseIntervalMilliseconds)
        {
            var testContext = new TestContext();

            var autoCloseInterval = TimeSpan.FromMilliseconds(autoCloseIntervalMilliseconds);
            var settings = new ManagedDoorSettings()
            {
                AutoCloseInterval = autoCloseInterval
            };

            var mockDoors = Enumerable.Repeat(0, doorCount)
                .Select(x => testContext.MakeFakeDoor(DoorStatus.Closed))
                .ToArray();

            foreach (var mockDoor in mockDoors)
                testContext.Uut.AddDoor(mockDoor.Object, settings);

            foreach (var mockDoor in mockDoors)
                testContext.Uut.ManagedDoors.ShouldContain(managedDoor =>
                    ReferenceEquals(managedDoor.Door, mockDoor.Object)
                        && (managedDoor.Settings.AutoCloseInterval == autoCloseInterval));
        }

        #endregion AddDoor() Tests

        #region ClearDoors() Tests

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void ClearDoors_Tests(int doorCount)
        {
            var testContext = new TestContext();

            var mockDoors = Enumerable.Repeat(0, doorCount)
                .Select(x => new Mock<IMyDoor>())
                .ToArray();
            foreach (var mockDoor in mockDoors)
                testContext.Uut.AddDoor(mockDoor.Object, new ManagedDoorSettings());

            testContext.Uut.ClearDoors();

            testContext.Uut.ManagedDoors.ShouldBeEmpty();
        }

        #endregion ClearDoors() Tests

        #region MakeManageDoorsOperation() Tests

        [Test]
        public void MakeManageDoorsOperation_ManagedDoorsIsEmpty_CompletesImmediately()
        {
            var testContext = new TestContext();

            testContext.Uut.MakeManageDoorsOperation()
                .ShouldRunToCompletionIn(1);
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        public void MakeManageDoorsOperation_ManagedDoorsIsNotEmpty_ManagesOneDoorPerExecution(int doorCount)
        {
            var testContext = new TestContext();

            var mockDoors = Enumerable.Repeat(0, doorCount)
                .Select(x => testContext.MakeFakeDoor(DoorStatus.Closed))
                .ToArray();
            foreach (var mockDoor in mockDoors)
                testContext.Uut.AddDoor(mockDoor.Object, new ManagedDoorSettings());

            testContext.Uut.MakeManageDoorsOperation()
                .ShouldRunToCompletionIn(doorCount);
        }

        [TestCase(1,  DoorStatus.Closing)]
        [TestCase(1,  DoorStatus.Closed)]
        [TestCase(10, DoorStatus.Closing)]
        [TestCase(10, DoorStatus.Closed)]
        public void MakeManageDoorsOperation_DoorIsClosed_EnablesDoor(int doorCount, DoorStatus doorStatus)
        {
            var testContext = new TestContext();

            var mockDoors = Enumerable.Repeat(0, doorCount)
                .Select(x => testContext.MakeFakeDoor(doorStatus))
                .ToArray();
            foreach (var mockDoor in mockDoors)
                testContext.Uut.AddDoor(mockDoor.Object, new ManagedDoorSettings());

            testContext.Uut.MakeManageDoorsOperation()
                .ShouldRunToCompletion();

            foreach (var mockDoor in mockDoors)
            {
                mockDoor.ShouldHaveReceivedSet(x => x.Enabled = true);
                mockDoor.ShouldNotHaveReceived(x => x.CloseDoor());
            }
        }

        [TestCase(1,  DoorStatus.Opening, 100)]
        [TestCase(1,  DoorStatus.Open,    200)]
        [TestCase(10, DoorStatus.Opening, 300)]
        [TestCase(10, DoorStatus.Open,    400)]
        public void MakeManageDoorsOperation_DoorIsNotClosedAndDoesNotNeedClosed_EnablesDoor(int doorCount, DoorStatus doorStatus, int autoCloseIntervalMilliseconds)
        {
            var testContext = new TestContext();

            var autoCloseInterval = TimeSpan.FromMilliseconds(autoCloseIntervalMilliseconds);
            var settings = new ManagedDoorSettings()
            {
                AutoCloseInterval = autoCloseInterval
            };

            var mockDoors = Enumerable.Repeat(0, doorCount)
                .Select(x => testContext.MakeFakeDoor(doorStatus))
                .ToArray();
            foreach (var mockDoor in mockDoors)
                testContext.Uut.AddDoor(mockDoor.Object, settings);

            testContext.Now += TimeSpan.FromMilliseconds(autoCloseIntervalMilliseconds);

            testContext.Uut.MakeManageDoorsOperation()
                .ShouldRunToCompletion();

            foreach (var mockDoor in mockDoors)
            {
                mockDoor.ShouldHaveReceivedSet(x => x.Enabled = true);
                mockDoor.ShouldNotHaveReceived(x => x.CloseDoor());
            }
        }

        [TestCase(1,  DoorStatus.Opening, 100)]
        [TestCase(10, DoorStatus.Open,    200)]
        [TestCase(1,  DoorStatus.Opening, 300)]
        [TestCase(10, DoorStatus.Open,    400)]
        public void MakeManageDoorsOperation_DoorNeedsClosed_ClosesDoor(int doorCount, DoorStatus doorStatus, int autoCloseIntervalMilliseconds)
        {
            var testContext = new TestContext();

            var autoCloseInterval = TimeSpan.FromMilliseconds(autoCloseIntervalMilliseconds);
            var settings = new ManagedDoorSettings()
            {
                AutoCloseInterval = autoCloseInterval
            };

            var mockDoors = Enumerable.Repeat(0, doorCount)
                .Select(x => testContext.MakeFakeDoor(doorStatus))
                .ToArray();
            foreach (var mockDoor in mockDoors)
                testContext.Uut.AddDoor(mockDoor.Object, settings);

            testContext.Now += TimeSpan.FromMilliseconds(autoCloseIntervalMilliseconds + 1);

            testContext.Uut.MakeManageDoorsOperation()
                .ShouldRunToCompletion();

            foreach (var mockDoor in mockDoors)
            {
                mockDoor.ShouldHaveReceivedSet(x => x.Enabled = true);
                mockDoor.ShouldHaveReceived(x => x.CloseDoor());
            }
        }

        [TestCase(1,  DoorStatus.Opening)]
        [TestCase(1,  DoorStatus.Open)]
        [TestCase(10, DoorStatus.Opening)]
        [TestCase(10, DoorStatus.Open)]
        public void MakeManageDoorsOperation_LockdownIsEnabledAndDoorNeedsClosed_ClosesDoor(int doorCount, DoorStatus doorStatus)
        {
            var testContext = new TestContext();

            var mockDoors = Enumerable.Repeat(0, doorCount)
                .Select(x => testContext.MakeFakeDoor(doorStatus))
                .ToArray();
            foreach (var mockDoor in mockDoors)
                testContext.Uut.AddDoor(mockDoor.Object, new ManagedDoorSettings());

            testContext.Uut.IsLockdownEnabled = true;

            testContext.Uut.MakeManageDoorsOperation()
                .ShouldRunToCompletion();

            foreach (var mockDoor in mockDoors)
            {
                mockDoor.ShouldHaveReceivedSet(x => x.Enabled = true);
                mockDoor.ShouldHaveReceived(x => x.CloseDoor());
            }
        }

        [TestCase(1, DoorStatus.Closing)]
        [TestCase(10, DoorStatus.Closing)]
        public void MakeManageDoorsOperation_LockdownIsEnabledAndDoorIsClosing_EnablesDoor(int doorCount, DoorStatus doorStatus)
        {
            var testContext = new TestContext();

            var mockDoors = Enumerable.Repeat(0, doorCount)
                .Select(x => testContext.MakeFakeDoor(doorStatus))
                .ToArray();
            foreach (var mockDoor in mockDoors)
                testContext.Uut.AddDoor(mockDoor.Object, new ManagedDoorSettings());

            testContext.Uut.IsLockdownEnabled = true;

            testContext.Uut.MakeManageDoorsOperation()
                .ShouldRunToCompletion();

            foreach (var mockDoor in mockDoors)
            {
                mockDoor.ShouldHaveReceivedSet(x => x.Enabled = true);
                mockDoor.ShouldNotHaveReceived(x => x.CloseDoor());
            }
        }

        [TestCase(1,  DoorStatus.Closed)]
        [TestCase(10, DoorStatus.Closed)]
        public void MakeManageDoorsOperation_LockdownIsEnabledAndDoorIsClosed_DisablesDoor(int doorCount, DoorStatus doorStatus)
        {
            var testContext = new TestContext();

            var mockDoors = Enumerable.Repeat(0, doorCount)
                .Select(x => testContext.MakeFakeDoor(doorStatus))
                .ToArray();
            foreach (var mockDoor in mockDoors)
                testContext.Uut.AddDoor(mockDoor.Object, new ManagedDoorSettings());

            testContext.Uut.IsLockdownEnabled = true;

            testContext.Uut.MakeManageDoorsOperation()
                .ShouldRunToCompletion();

            foreach (var mockDoor in mockDoors)
            {
                mockDoor.ShouldHaveReceivedSet(x => x.Enabled = false);
                mockDoor.ShouldNotHaveReceived(x => x.CloseDoor());
            }
        }

        [TestCase(DoorStatus.Opening)]
        [TestCase(DoorStatus.Open)]
        public void MakeManageDoorsOperation_DoorHasOpened_ClosesAfterAutoCloseInterval(DoorStatus doorStatus)
        {
            var testContext = new TestContext();

            var mockDoor = new Mock<IMyDoor>();
            mockDoor
                .Setup(x => x.Status)
                .Returns(DoorStatus.Closed);

            var autoCloseInterval = TimeSpan.FromSeconds(1);
            testContext.Uut.AddDoor(mockDoor.Object, new ManagedDoorSettings()
            {
                AutoCloseInterval = autoCloseInterval
            });

            mockDoor
                .Setup(x => x.Status)
                .Returns(doorStatus);

            testContext.Uut.MakeManageDoorsOperation()
                .ShouldRunToCompletion();

            mockDoor.Invocations.Clear();

            testContext.Now += autoCloseInterval;
            testContext.Now += TimeSpan.FromMilliseconds(1);

            testContext.Uut.MakeManageDoorsOperation()
                .ShouldRunToCompletion();

            mockDoor.ShouldHaveReceivedSet(x => x.Enabled = true);
            mockDoor.ShouldHaveReceived(x => x.CloseDoor());
        }

        [TestCase(100, 200, 300, 400)]
        public void MakeManageDoorsOperation_DoorSettingsAreDifferent_ClosesDoorsInSequence(params int[] autoCloseIntervalsMilliseconds)
        {
            var testContext = new TestContext();

            var mockDoors = Enumerable.Repeat(0, autoCloseIntervalsMilliseconds.Length)
                .Select(x => testContext.MakeFakeDoor(DoorStatus.Open))
                .ToArray();
            foreach (var i in Enumerable.Range(0, autoCloseIntervalsMilliseconds.Length))
                testContext.Uut.AddDoor(mockDoors[i].Object, new ManagedDoorSettings()
                {
                    AutoCloseInterval = TimeSpan.FromMilliseconds(autoCloseIntervalsMilliseconds[i])
                });

            var startTime = testContext.Now;
            foreach (var i in Enumerable.Range(0, autoCloseIntervalsMilliseconds.Length))
            {
                testContext.Now = startTime + TimeSpan.FromMilliseconds(autoCloseIntervalsMilliseconds[i] + 1);

                testContext.Uut.MakeManageDoorsOperation()
                    .ShouldRunToCompletion();

                mockDoors[i].ShouldHaveReceivedSet(x => x.Enabled = true);
                mockDoors[i].ShouldHaveReceived(x => x.CloseDoor());

                foreach(var j in Enumerable.Range(0, mockDoors.Length).Where(j => j != i))
                {
                    mockDoors[j].ShouldHaveReceivedSet(x => x.Enabled = true);
                    mockDoors[j].ShouldNotHaveReceived(x => x.CloseDoor());
                }

                mockDoors[i].Invocations.Clear();
            }
        }

        [Test]
        public void MakeManageDoorsOperation_OperationIsDisposed_RecyclesOperation()
        {
            var testContext = new TestContext();

            var mockDoors = Enumerable.Repeat(0, 10)
                .Select(_ =>
                {
                    var mockDoor = new Mock<IMyDoor>();

                    mockDoor
                        .Setup(x => x.Status)
                        .Returns(DoorStatus.Open);

                    return mockDoor;
                })
                .ToArray();
            foreach (var mockDoor in mockDoors)
                testContext.Uut.AddDoor(mockDoor.Object, new ManagedDoorSettings());

            mockDoors
                .ForEach(x => x.Invocations.Clear());

            testContext.MockLogger
                .Invocations.Clear();

            var result = testContext.Uut.MakeManageDoorsOperation();
            result.ShouldRunToCompletion();

            var mockDoorsInvocations = mockDoors
                .Select(x => x.Invocations.ToArray())
                .ToArray();

            var mockLoggerInvocations = testContext.MockLogger
                .Invocations.ToArray();

            result.ShouldBeAssignableTo<IDisposable>();
            (result as IDisposable).Dispose();

            mockDoors
                .ForEach(x => x.Invocations.Clear());

            testContext.MockLogger
                .Invocations.Clear();

            testContext.Uut.MakeManageDoorsOperation()
                .ShouldBeSameAs(result);

            result.ShouldRunToCompletion();

            foreach (var i in Enumerable.Range(0, mockDoorsInvocations.Length))
            {
                mockDoors[i].Invocations.Count.ShouldBe(mockDoorsInvocations[i].Length);
                foreach (var j in Enumerable.Range(0, mockDoorsInvocations[i].Length))
                    mockDoors[i].Invocations[j].ShouldBe(mockDoorsInvocations[i][j]);
            }

            testContext.MockLogger.Invocations.Count.ShouldBe(mockLoggerInvocations.Length);
            foreach (var i in Enumerable.Range(0, mockLoggerInvocations.Length))
                testContext.MockLogger.Invocations[i].ShouldBe(mockLoggerInvocations[i]);
        }

        #endregion MakeManageDoorsOperation() Tests
    }
}
