﻿using System;

using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public class BatteryBlockCollectionHandler : IBlockCollectionHandler
        {
            public BatteryBlockCollectionHandler(
                IBatteryBlockManager batteryBlockManager,
                ILogger logger)
            {
                _batteryBlockManager = batteryBlockManager;
                _logger = logger;

                _collectBatteryBlockOperationPool = new ObjectPool<CollectBatteryBlockOperation>(onFinished
                    => new CollectBatteryBlockOperation(this, onFinished));
            }

            public void OnStarting()
                => _batteryBlockManager.ClearBatteryBlocks();

            public IBackgroundOperation<BlockCollectionResult> MakeCollectBlockOperation(IMyTerminalBlock block)
            {
                var collectBatteryOperation = _collectBatteryBlockOperationPool.Get();

                collectBatteryOperation.Block = block;

                return collectBatteryOperation;
            }

            public void OnCompleted()
                => _logger.AddLine($"Discovered {_batteryBlockManager.BatteryBlocks.Count} batteries for management");

            private readonly IBatteryBlockManager _batteryBlockManager;

            private readonly ILogger _logger;

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

                    var batteryBlock = Block as IMyBatteryBlock;
                    if (batteryBlock != null)
                    {
                        _owner._batteryBlockManager.AddBatteryBlock(batteryBlock);
                        _result = BlockCollectionResult.Success;
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
