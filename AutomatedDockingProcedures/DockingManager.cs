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

            private readonly ObjectPool<DockOperation> _dockOperationPool;

            private readonly ObjectPool<UndockOperation> _undockOperationPool;

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

                    if (Owner._connectorManager.Connectors.Count == 0)
                    {
                        Owner._logger.AddLine(" - Docking failed: No Connectors loaded");
                        return BackgroundOperationResult.Completed;
                    }

                    var anyConnectable = false;
                    for (var i = 0; i < Owner._connectorManager.Connectors.Count; ++i)
                    {
                        var status = Owner._connectorManager.Connectors[i].Status;

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

                    subOperationScheduler.Invoke(Owner._connectorManager.MakeConnectOperation());
                    subOperationScheduler.Invoke(Owner._landingGearManager.MakeLockOperation());
                    subOperationScheduler.Invoke(Owner._batteryBlockManager.MakeRechargeOperation());
                    subOperationScheduler.Invoke(Owner._gasTankManager.MakeStockpileOperation());
                    subOperationScheduler.Invoke(Owner._functionalBlockManager.MakeDisableOperation());

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

                    if (Owner._connectorManager.Connectors.Count == 0)
                    {
                        Owner._logger.AddLine(" - Undocking failed: No Connectors loaded");
                        return BackgroundOperationResult.Completed;
                    }

                    var anyConnected = false;
                    for (var i = 0; i < Owner._connectorManager.Connectors.Count; ++i)
                        anyConnected |= (Owner._connectorManager.Connectors[i].Status == MyShipConnectorStatus.Connected);

                    if (!anyConnected)
                    {
                        Owner._logger.AddLine(" - Docking failed: Ship is not currently docked");
                        return BackgroundOperationResult.Completed;
                    }

                    subOperationScheduler.Invoke(Owner._functionalBlockManager.MakeEnableOperation());
                    subOperationScheduler.Invoke(Owner._gasTankManager.MakeDispenseOperation());
                    subOperationScheduler.Invoke(Owner._batteryBlockManager.MakeDischargeOperation());
                    subOperationScheduler.Invoke(Owner._landingGearManager.MakeUnlockOperation());
                    subOperationScheduler.Invoke(Owner._connectorManager.MakeDisconnectOperation());

                    return BackgroundOperationResult.NotCompleted;
                }

                protected internal override void OnCompleted()
                    => Owner._logger.AddLine(" - Undocking complete");
            }
        }
    }
}
