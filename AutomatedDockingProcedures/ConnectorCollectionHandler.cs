using System;

using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public class ConnectorCollectionHandler : IBlockCollectionHandler
        {
            public ConnectorCollectionHandler(
                IConnectorManager connectorManager,
                ILogger logger)
            {
                _connectorManager = connectorManager;
                _logger = logger;

                _collectConnectorOperationPool = new ObjectPool<CollectConnectorOperation>(onFinished
                    => new CollectConnectorOperation(this, onFinished));
            }

            public void OnStarting()
                => _connectorManager.ClearConnectors();

            public IBackgroundOperation<BlockCollectionResult> MakeCollectBlockOperation(IMyTerminalBlock block)
            {
                var collectConnectorOperation = _collectConnectorOperationPool.Get();

                collectConnectorOperation.Block = block;

                return collectConnectorOperation;
            }

            public void OnCompleted()
            {
                if(_connectorManager.Connectors.Count > 0)
                    _logger.AddLine($"Discovered {_connectorManager.Connectors.Count} connectors for management");
            }

            private readonly IConnectorManager _connectorManager;

            private readonly ILogger _logger;

            private readonly ObjectPool<CollectConnectorOperation> _collectConnectorOperationPool;

            private class CollectConnectorOperation : IBackgroundOperation<BlockCollectionResult>, IDisposable
            {
                public CollectConnectorOperation(
                    ConnectorCollectionHandler owner,
                    Action onDisposed)
                {
                    _owner = owner;
                    _onDisposed = onDisposed;
                }

                public IMyTerminalBlock Block;

                public BlockCollectionResult Result
                    => _result;

                public BackgroundOperationResult Execute(Action<IBackgroundOperation> subOperationScheduler)
                {
                    _result = BlockCollectionResult.Ignored;

                    var connector = Block as IMyShipConnector;
                    if (connector != null)
                    {
                        _owner._connectorManager.AddConnector(connector);
                        _result = BlockCollectionResult.Success;
                    }

                    return BackgroundOperationResult.Completed;
                }

                public void Dispose()
                {
                    Block = null;
                    _onDisposed.Invoke();
                }

                private readonly ConnectorCollectionHandler _owner;

                private readonly Action _onDisposed;

                private BlockCollectionResult _result;
            }
        }
    }
}
