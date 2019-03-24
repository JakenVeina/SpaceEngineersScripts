using System;

using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public class BatteryBlockCollectionHandler : IBlockCollectionHandler
        {
            public BatteryBlockCollectionHandler(
                IBatteryBlockManager batteryBlockManager,
                ILogger logger,
                IDockingManagerSettingsProvider dockingManagerSettingsProvider)
            {
                _batteryBlockManager = batteryBlockManager;
                _logger = logger;
                _dockingManagerSettingsProvider = dockingManagerSettingsProvider;

                _collectBatteryBlockOperationPool = new ObjectPool<CollectBatteryBlockOperation>(onFinished
                    => new CollectBatteryBlockOperation(this, onFinished));
            }

            public void OnStarting()
                => _batteryBlockManager.ClearBlocks();

            public IBackgroundOperation<BlockCollectionResult> MakeCollectBlockOperation(IMyTerminalBlock block)
            {
                var collectBatteryBlockOperation = _collectBatteryBlockOperationPool.Get();

                collectBatteryBlockOperation.Block = block;

                return collectBatteryBlockOperation;
            }

            public void OnCompleted()
            {
                if(_batteryBlockManager.Blocks.Count > 0)
                    _logger.AddLine($"Discovered {_batteryBlockManager.Blocks.Count} batteries for management");
            }

            private readonly IBatteryBlockManager _batteryBlockManager;

            private readonly ILogger _logger;

            private readonly IDockingManagerSettingsProvider _dockingManagerSettingsProvider;

            private readonly ObjectPool<CollectBatteryBlockOperation> _collectBatteryBlockOperationPool;

            private class CollectBatteryBlockOperation : IBackgroundOperation<BlockCollectionResult>, IDisposable
            {
                public CollectBatteryBlockOperation(
                    BatteryBlockCollectionHandler owner,
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

                    if (!_owner._dockingManagerSettingsProvider.Settings.IgnoreBatteryBlocks)
                    {
                        var batteryBlock = Block as IMyBatteryBlock;
                        if (batteryBlock != null)
                        {
                            _owner._batteryBlockManager.AddBlock(batteryBlock);
                            _result = BlockCollectionResult.Success;
                        }
                    }

                    return BackgroundOperationResult.Completed;
                }

                public void Dispose()
                {
                    Block = null;
                    _onDisposed.Invoke();
                }

                private readonly BatteryBlockCollectionHandler _owner;

                private readonly Action _onDisposed;

                private BlockCollectionResult _result;
            }
        }
    }
}
