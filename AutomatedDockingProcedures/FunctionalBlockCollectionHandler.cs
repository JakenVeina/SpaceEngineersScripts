using System;

using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public class FunctionalBlockCollectionHandler : IBlockCollectionHandler
        {
            public FunctionalBlockCollectionHandler(
                IFunctionalBlockManager functionalBlockManager,
                ILogger logger)
            {
                _functionalBlockManager = functionalBlockManager;
                _logger = logger;

                _collectFunctionalBlockOperationPool = new ObjectPool<CollectFunctionalBlockOperation>(onFinished
                    => new CollectFunctionalBlockOperation(this, onFinished));
            }

            public void OnStarting()
                => _functionalBlockManager.ClearFunctionalBlocks();

            public IBackgroundOperation<BlockCollectionResult> MakeCollectBlockOperation(IMyTerminalBlock block)
            {
                var collectFunctionalBlockOperation = _collectFunctionalBlockOperationPool.Get();

                collectFunctionalBlockOperation.Block = block;

                return collectFunctionalBlockOperation;
            }

            public void OnCompleted()
                => _logger.AddLine($"Discovered {_functionalBlockManager.FunctionalBlocks.Count} batteries for management");

            private readonly IFunctionalBlockManager _functionalBlockManager;

            private readonly ILogger _logger;

            private readonly ObjectPool<CollectFunctionalBlockOperation> _collectFunctionalBlockOperationPool;

            private class CollectFunctionalBlockOperation : IBackgroundOperation<BlockCollectionResult>, IDisposable
            {
                public CollectFunctionalBlockOperation(
                    FunctionalBlockCollectionHandler owner,
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

                    var gyro = Block as IMyGyro;
                    if (gyro != null)
                    {
                        _owner._functionalBlockManager.AddFunctionalBlock(gyro);
                        _result = BlockCollectionResult.Success;
                        return BackgroundOperationResult.Completed;
                    }

                    var lightingBlock = Block as IMyLightingBlock;
                    if (lightingBlock != null)
                    {
                        _owner._functionalBlockManager.AddFunctionalBlock(lightingBlock);
                        _result = BlockCollectionResult.Success;
                        return BackgroundOperationResult.Completed;
                    }

                    var beacon = Block as IMyBeacon;
                    if (beacon != null)
                    {
                        _owner._functionalBlockManager.AddFunctionalBlock(beacon);
                        _result = BlockCollectionResult.Success;
                        return BackgroundOperationResult.Completed;
                    }

                    var antenna = Block as IMyRadioAntenna;
                    if (antenna != null)
                    {
                        _owner._functionalBlockManager.AddFunctionalBlock(antenna);
                        _result = BlockCollectionResult.Success;
                        return BackgroundOperationResult.Completed;
                    }

                    var gasGenerator = Block as IMyGasGenerator;
                    if (gasGenerator != null)
                    {
                        _owner._functionalBlockManager.AddFunctionalBlock(gasGenerator);
                        _result = BlockCollectionResult.Success;
                    }

                    var reactor = Block as IMyReactor;
                    if (reactor != null)
                    {
                        _owner._functionalBlockManager.AddFunctionalBlock(reactor);
                        _result = BlockCollectionResult.Success;
                        return BackgroundOperationResult.Completed;
                    }

                    return BackgroundOperationResult.Completed;
                }

                public void Dispose()
                {
                    Block = null;
                    _onDisposed.Invoke();
                }

                private readonly FunctionalBlockCollectionHandler _owner;

                private readonly Action _onDisposed;

                private BlockCollectionResult _result;
            }
        }
    }
}
