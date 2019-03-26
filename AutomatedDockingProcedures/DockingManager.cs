using System;

using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public interface IDockingManager
        {
            IBackgroundOperation MakeDockOperation();

            IBackgroundOperation MakeUndockOperation();

            IBackgroundOperation MakeToggleOperation();
        }

        public class DockingManager : IDockingManager
        {
            public DockingManager(
                ILogger logger,
                IBatteryBlockManager batteryBlockManager,
                IConnectorManager connectorManager,
                IFunctionalBlockManager functionalBlockManager,
                IGasTankManager gasTankManager,
                ILandingGearManager landingGearManager)
            {
                _dockOperationPool = new ObjectPool<DockOperation>(onFinished
                    => new DockOperation(this, onFinished));

                _undockOperationPool = new ObjectPool<UndockOperation>(onFinished
                    => new UndockOperation(this, onFinished));

                _toggleOperationPool = new ObjectPool<ToggleOperation>(onFinished
                    => new ToggleOperation(this, onFinished));

                _logger = logger;
                _batteryBlockManager = batteryBlockManager;
                _connectorManager = connectorManager;
                _functionalBlockManager = functionalBlockManager;
                _gasTankManager = gasTankManager;
                _landingGearManager = landingGearManager;
            }

            public IBackgroundOperation MakeDockOperation()
                => _dockOperationPool.Get();

            public IBackgroundOperation MakeUndockOperation()
                => _undockOperationPool.Get();

            public IBackgroundOperation MakeToggleOperation()
                => _toggleOperationPool.Get();

            private readonly ObjectPool<DockOperation> _dockOperationPool;

            private readonly ObjectPool<UndockOperation> _undockOperationPool;

            private readonly ObjectPool<ToggleOperation> _toggleOperationPool;

            private readonly ILogger _logger;

            private readonly IBatteryBlockManager _batteryBlockManager;

            private readonly IConnectorManager _connectorManager;

            private readonly IFunctionalBlockManager _functionalBlockManager;

            private readonly IGasTankManager _gasTankManager;

            private readonly ILandingGearManager _landingGearManager;

            private abstract class DockingOperationBase : IBackgroundOperation, IDisposable
            {
                public DockingOperationBase(DockingManager owner, Action onDisposed)
                {
                    _owner = owner;
                    _onDisposed = onDisposed;

                    Reset();
                }

                public BackgroundOperationResult Execute(Action<IBackgroundOperation> subOperationScheduler)
                {
                    if(!_hasStarted)
                    {
                        var result = OnStarting(subOperationScheduler);
                        _hasStarted = true;
                        return result;
                    }

                    OnCompleted();
                    return BackgroundOperationResult.Completed;
                }

                public void Dispose()
                {
                    Reset();
                    _onDisposed.Invoke();
                }

                internal protected DockingManager Owner
                    => _owner;

                internal protected abstract BackgroundOperationResult OnStarting(Action<IBackgroundOperation> subOperationScheduler);

                internal protected abstract void OnCompleted();

                private void Reset()
                    => _hasStarted = false;

                private readonly DockingManager _owner;

                private readonly Action _onDisposed;

                private bool _hasStarted;
            }

            private sealed class DockOperation : DockingOperationBase
            {
                public DockOperation(DockingManager owner, Action onDisposed)
                    : base(owner, onDisposed) { }

                internal protected override BackgroundOperationResult OnStarting(Action<IBackgroundOperation> subOperationScheduler)
                {
                    Owner._logger.AddLine("Attempting to Dock...");

                    if (Owner._connectorManager.Blocks.Count == 0)
                    {
                        Owner._logger.AddLine(" - Docking failed: No Connectors loaded");
                        return BackgroundOperationResult.Completed;
                    }

                    var anyConnectable = false;
                    for (var i = 0; i < Owner._connectorManager.Blocks.Count; ++i)
                    {
                        var status = Owner._connectorManager.Blocks[i].Status;

                        if (status == MyShipConnectorStatus.Connected)
                        {
                            Owner._logger.AddLine(" - Docking failed: Ship is already docked");
                            return BackgroundOperationResult.Completed;
                        }

                        anyConnectable |= (status == MyShipConnectorStatus.Connectable);
                    }

                    if (!anyConnectable)
                    {
                        Owner._logger.AddLine(" - Docking failed: No Connectors in range");
                        return BackgroundOperationResult.Completed;
                    }

                    subOperationScheduler.Invoke(Owner._connectorManager.MakeOnDockingOperation());
                    subOperationScheduler.Invoke(Owner._landingGearManager.MakeOnDockingOperation());
                    subOperationScheduler.Invoke(Owner._batteryBlockManager.MakeOnDockingOperation());
                    subOperationScheduler.Invoke(Owner._gasTankManager.MakeOnDockingOperation());
                    subOperationScheduler.Invoke(Owner._functionalBlockManager.MakeOnDockingOperation());

                    return BackgroundOperationResult.NotCompleted;
                }

                protected internal override void OnCompleted()
                    => Owner._logger.AddLine(" - Docking complete");
            }

            private sealed class UndockOperation : DockingOperationBase
            {
                public UndockOperation(DockingManager owner, Action onDisposed)
                    : base(owner, onDisposed) { }

                internal protected override BackgroundOperationResult OnStarting(Action<IBackgroundOperation> subOperationScheduler)
                {
                    Owner._logger.AddLine("Executing Undocking Procedure...");

                    if (Owner._connectorManager.Blocks.Count == 0)
                    {
                        Owner._logger.AddLine(" - Undocking failed: No Connectors loaded");
                        return BackgroundOperationResult.Completed;
                    }

                    var anyConnected = false;
                    for (var i = 0; i < Owner._connectorManager.Blocks.Count; ++i)
                        if (Owner._connectorManager.Blocks[i].Status == MyShipConnectorStatus.Connected)
                        {
                            anyConnected = true;
                            break;
                        }

                    if (!anyConnected)
                    {
                        Owner._logger.AddLine(" - Docking failed: Ship is not currently docked");
                        return BackgroundOperationResult.Completed;
                    }

                    subOperationScheduler.Invoke(Owner._functionalBlockManager.MakeOnUndockingOperation());
                    subOperationScheduler.Invoke(Owner._gasTankManager.MakeOnUndockingOperation());
                    subOperationScheduler.Invoke(Owner._batteryBlockManager.MakeOnUndockingOperation());
                    subOperationScheduler.Invoke(Owner._landingGearManager.MakeOnUndockingOperation());
                    subOperationScheduler.Invoke(Owner._connectorManager.MakeOnUndockingOperation());

                    return BackgroundOperationResult.NotCompleted;
                }

                protected internal override void OnCompleted()
                    => Owner._logger.AddLine(" - Undocking complete");
            }

            private sealed class ToggleOperation : DockingOperationBase
            {
                public ToggleOperation(DockingManager owner, Action onDisposed)
                    : base(owner, onDisposed) { }

                internal protected override BackgroundOperationResult OnStarting(Action<IBackgroundOperation> subOperationScheduler)
                {
                    if (Owner._connectorManager.Blocks.Count == 0)
                    {
                        Owner._logger.AddLine(" - Toggle failed: No Connectors loaded");
                        return BackgroundOperationResult.Completed;
                    }

                    var anyConnected = false;
                    var anyConnectable = false;
                    for (var i = 0; i < Owner._connectorManager.Blocks.Count; ++i)
                    {
                        var status = Owner._connectorManager.Blocks[i].Status;

                        if (status == MyShipConnectorStatus.Connected)
                        {
                            anyConnected = true;
                            break;
                        }

                        anyConnectable |= (status == MyShipConnectorStatus.Connectable);
                    }

                    if (anyConnected)
                    {
                        _isDocking = false;
                        Owner._logger.AddLine("Executing Undocking Procedure...");
                        subOperationScheduler.Invoke(Owner._functionalBlockManager.MakeOnUndockingOperation());
                        subOperationScheduler.Invoke(Owner._gasTankManager.MakeOnUndockingOperation());
                        subOperationScheduler.Invoke(Owner._batteryBlockManager.MakeOnUndockingOperation());
                        subOperationScheduler.Invoke(Owner._landingGearManager.MakeOnUndockingOperation());
                        subOperationScheduler.Invoke(Owner._connectorManager.MakeOnUndockingOperation());
                    }
                    else if(anyConnectable)
                    {
                        _isDocking = true;
                        Owner._logger.AddLine("Executing Docking Procedure...");
                        subOperationScheduler.Invoke(Owner._connectorManager.MakeOnDockingOperation());
                        subOperationScheduler.Invoke(Owner._landingGearManager.MakeOnDockingOperation());
                        subOperationScheduler.Invoke(Owner._batteryBlockManager.MakeOnDockingOperation());
                        subOperationScheduler.Invoke(Owner._gasTankManager.MakeOnDockingOperation());
                        subOperationScheduler.Invoke(Owner._functionalBlockManager.MakeOnDockingOperation());
                    }
                    else
                    {
                        Owner._logger.AddLine(" - Toggle failed: Ship is not currently docked or dockable");
                        return BackgroundOperationResult.Completed;
                    }

                    return BackgroundOperationResult.NotCompleted;
                }

                protected internal override void OnCompleted()
                    => Owner._logger.AddLine(_isDocking
                        ? " - Docking complete"
                        : " - Undocking complete");

                private bool _isDocking;
            }
        }
    }
}
