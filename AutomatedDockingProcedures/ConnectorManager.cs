using System;
using System.Collections.Generic;

using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public interface IConnectorManager
        {
            IReadOnlyList<IMyShipConnector> Connectors { get; }

            void AddConnector(IMyShipConnector connector);

            void ClearConnectors();

            IBackgroundOperation MakeConnectOperation();

            IBackgroundOperation MakeDisconnectOperation();
        }

        public class ConnectorManager : IConnectorManager
        {
            public ConnectorManager()
            {
                _connectOperationPool = new ObjectPool<ConnectOperation>(onFinished
                    => new ConnectOperation(this, onFinished));

                _disconnectOperationPool = new ObjectPool<DisconnectOperation>(onFinished
                    => new DisconnectOperation(this, onFinished));
            }

            public IReadOnlyList<IMyShipConnector> Connectors
                => _connectors;

            public void AddConnector(IMyShipConnector connector)
                => _connectors.Add(connector);

            public void ClearConnectors()
                => _connectors.Clear();

            public IBackgroundOperation MakeConnectOperation()
                => _connectOperationPool.Get();

            public IBackgroundOperation MakeDisconnectOperation()
                => _disconnectOperationPool.Get();

            private readonly List<IMyShipConnector> _connectors
                = new List<IMyShipConnector>();

            private readonly ObjectPool<ConnectOperation> _connectOperationPool;

            private readonly ObjectPool<DisconnectOperation> _disconnectOperationPool;

            private abstract class ConnectorOperationBase : IBackgroundOperation, IDisposable
            {
                public ConnectorOperationBase(ConnectorManager owner, Action onDisposed)
                {
                    _owner = owner;
                    _onDisposed = onDisposed;

                    Reset();
                }

                public BackgroundOperationResult Execute(Action<IBackgroundOperation> subOperationScheduler)
                {
                    if(_owner._connectors.Count == 0)
                        return BackgroundOperationResult.Completed;

                    OnExecuting(_owner._connectors[_connectorIndex]);

                    if (++_connectorIndex < _owner._connectors.Count)
                        return BackgroundOperationResult.NotCompleted;

                    return BackgroundOperationResult.Completed;
                }

                public void Dispose()
                {
                    Reset();
                    _onDisposed.Invoke();
                }

                protected abstract void OnExecuting(IMyShipConnector connector);

                private void Reset()
                    => _connectorIndex = 0;

                private readonly ConnectorManager _owner;

                private readonly Action _onDisposed;

                private int _connectorIndex;
            }

            private sealed class ConnectOperation : ConnectorOperationBase
            {
                public ConnectOperation(ConnectorManager owner, Action onDisposed)
                    : base(owner, onDisposed) { }

                protected override void OnExecuting(IMyShipConnector connector)
                    => connector.Connect();
            }

            private sealed class DisconnectOperation : ConnectorOperationBase
            {
                public DisconnectOperation(ConnectorManager owner, Action onDisposed)
                    : base(owner, onDisposed) { }

                protected override void OnExecuting(IMyShipConnector connector)
                    => connector.Disconnect();
            }
        }
    }
}
