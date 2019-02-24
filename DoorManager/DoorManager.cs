using System;
using System.Collections.Generic;

using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    public partial class Program
    {
        public interface IDoorManager
        {
            int DoorCount { get; }

            bool IsLockdownEnabled { get; set; }

            void AddDoor(IMyDoor door, ManagedDoorSettings settings);

            void ClearDoors();

            IBackgroundOperation MakeManageDoorsOperation();
        }

        public class DoorManager : IDoorManager
        {
            public DoorManager(
                IDateTimeProvider dateTimeProvider,
                IDoorManagerSettingsProvider doorManagerSettingsProvider,
                ILogger logger)
            {
                _dateTimeProvider = dateTimeProvider;
                _doorManagerSettingsProvider = doorManagerSettingsProvider;
                _logger = logger;

                _manageDoorsOperationPool = new ObjectPool<ManageDoorsOperation>(onFinished
                    => new ManageDoorsOperation(this, onFinished));
            }

            public int DoorCount
                => _managedDoors.Count;

            public bool IsLockdownEnabled
            {
                get { return _isLockdownEnabled; }
                set
                {
                    if (!_isLockdownEnabled && value)
                        _logger.AddLine("Engaging Lockdown");
                    else if (_isLockdownEnabled && !value)
                        _logger.AddLine("Releasing Lockdown");

                    _isLockdownEnabled = value;
                }
            }

            public void AddDoor(IMyDoor door, ManagedDoorSettings settings)
                => _managedDoors.Add(new ManagedDoor()
                {
                    Door = door,
                    Settings = settings,
                    LastStatus = door.Status,
                    CloseAfter = _dateTimeProvider.Now + settings.AutoCloseInterval,
                });

            public void ClearDoors()
                => _managedDoors.Clear();

            public IBackgroundOperation MakeManageDoorsOperation()
                => _manageDoorsOperationPool.Get();

            internal protected IReadOnlyList<ManagedDoor> ManagedDoors
                => _managedDoors;

            private readonly IDateTimeProvider _dateTimeProvider;

            private readonly IDoorManagerSettingsProvider _doorManagerSettingsProvider;

            private readonly ILogger _logger;

            private readonly ObjectPool<ManageDoorsOperation> _manageDoorsOperationPool;

            private readonly List<ManagedDoor> _managedDoors
                = new List<ManagedDoor>();

            private bool _isLockdownEnabled
                = false;

            internal protected struct ManagedDoor
            {
                public IMyDoor Door;

                public ManagedDoorSettings Settings;

                public DoorStatus LastStatus;

                public DateTime CloseAfter;
            }

            private class ManageDoorsOperation : IBackgroundOperation, IDisposable
            {
                public ManageDoorsOperation(DoorManager owner, Action onDisposed)
                {
                    _owner = owner;
                    _onDisposed = onDisposed;

                    Reset();
                }

                public BackgroundOperationResult Execute(Action<IBackgroundOperation> subOperationScheduler)
                {
                    if (_owner._managedDoors.Count == 0)
                        return BackgroundOperationResult.Completed;

                    var managedDoor = _owner._managedDoors[_currentDoorIndex];

                    if (_owner._isLockdownEnabled)
                    {
                        if ((managedDoor.Door.Status == DoorStatus.Opening) || (managedDoor.Door.Status == DoorStatus.Open))
                        {
                            managedDoor.Door.Enabled = true;
                            managedDoor.Door.CloseDoor();
                            _owner._logger.AddLine($"Closing \"{managedDoor.Door.CustomName}\"");
                        }
                        if (managedDoor.Door.Status == DoorStatus.Closing)
                            managedDoor.Door.Enabled = true;
                        else
                            managedDoor.Door.Enabled = false;
                    }
                    else
                    {
                        managedDoor.Door.Enabled = true;

                        if ((managedDoor.LastStatus == DoorStatus.Closed) && (managedDoor.Door.Status != DoorStatus.Closed))
                            managedDoor.CloseAfter = _owner._dateTimeProvider.Now + managedDoor.Settings.AutoCloseInterval;

                        if ((_owner._dateTimeProvider.Now > managedDoor.CloseAfter) && ((managedDoor.Door.Status == DoorStatus.Opening) || (managedDoor.Door.Status == DoorStatus.Open)))
                        {
                            managedDoor.Door.CloseDoor();
                            _owner._logger.AddLine($"Closing \"{managedDoor.Door.CustomName}\"");
                        }
                    }

                    managedDoor.LastStatus = managedDoor.Door.Status;

                    _owner._managedDoors[_currentDoorIndex] = managedDoor;

                    return new BackgroundOperationResult(++_currentDoorIndex >= _owner._managedDoors.Count);
                }

                public void Dispose()
                {
                    Reset();
                    _onDisposed.Invoke();
                }

                private void Reset()
                    => _currentDoorIndex = 0;

                private readonly DoorManager _owner;

                private readonly Action _onDisposed;

                private int _currentDoorIndex;
            }
        }
    }
}
