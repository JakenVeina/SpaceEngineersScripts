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
                ILogger logger,
                IDockingManagerSettingsProvider dockingManagerSettingsProvider)
            {
                _functionalBlockManager = functionalBlockManager;
                _logger = logger;
                _dockingManagerSettingsProvider = dockingManagerSettingsProvider;

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
            {
                if(_functionalBlockManager.BeaconCount > 0)
                    _logger.AddLine($"Discovered {_functionalBlockManager.BeaconCount} beacons for management");
                if(_functionalBlockManager.GasGeneratorCount > 0)
                    _logger.AddLine($"Discovered {_functionalBlockManager.GasGeneratorCount} gas generators for management");
                if(_functionalBlockManager.GyroCount > 0)
                    _logger.AddLine($"Discovered {_functionalBlockManager.GyroCount} gyroscopes for management");
                if(_functionalBlockManager.LightingBlockCount > 0)
                    _logger.AddLine($"Discovered {_functionalBlockManager.LightingBlockCount} lighting blocks for management");
                if(_functionalBlockManager.RadioAntennaCount > 0)
                    _logger.AddLine($"Discovered {_functionalBlockManager.RadioAntennaCount} radio antennae for management");
                if(_functionalBlockManager.ReactorCount > 0)
                    _logger.AddLine($"Discovered {_functionalBlockManager.ReactorCount} reactors for management");

            }

            private readonly IFunctionalBlockManager _functionalBlockManager;

            private readonly ILogger _logger;

            private readonly IDockingManagerSettingsProvider _dockingManagerSettingsProvider;

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

                    if (!_owner._dockingManagerSettingsProvider.Settings.IgnoreGyros)
                    {
                        var gyro = Block as IMyGyro;
                        if (gyro != null)
                        {
                            _owner._functionalBlockManager.AddFunctionalBlock(gyro);
                            _result = BlockCollectionResult.Success;
                            return BackgroundOperationResult.Completed;
                        }
                    }

                    if (!_owner._dockingManagerSettingsProvider.Settings.IgnoreLightingBlocks)
                    {
                        var lightingBlock = Block as IMyLightingBlock;
                        if (lightingBlock != null)
                        {
                            _owner._functionalBlockManager.AddFunctionalBlock(lightingBlock);
                            _result = BlockCollectionResult.Success;
                            return BackgroundOperationResult.Completed;
                        }
                    }

                    if (!_owner._dockingManagerSettingsProvider.Settings.IgnoreBeacons)
                    {
                        var beacon = Block as IMyBeacon;
                        if (beacon != null)
                        {
                            _owner._functionalBlockManager.AddFunctionalBlock(beacon);
                            _result = BlockCollectionResult.Success;
                            return BackgroundOperationResult.Completed;
                        }
                    }

                    if (!_owner._dockingManagerSettingsProvider.Settings.IgnoreRadioAntennae)
                    {
                        var antenna = Block as IMyRadioAntenna;
                        if (antenna != null)
                        {
                            _owner._functionalBlockManager.AddFunctionalBlock(antenna);
                            _result = BlockCollectionResult.Success;
                            return BackgroundOperationResult.Completed;
                        }
                    }

                    if (!_owner._dockingManagerSettingsProvider.Settings.IgnoreGasGenerators)
                    {
                        var gasGenerator = Block as IMyGasGenerator;
                        if (gasGenerator != null)
                        {
                            _owner._functionalBlockManager.AddFunctionalBlock(gasGenerator);
                            _result = BlockCollectionResult.Success;
                            return BackgroundOperationResult.Completed;
                        }
                    }

                    if (!_owner._dockingManagerSettingsProvider.Settings.IgnoreReactors)
                    {
                        var reactor = Block as IMyReactor;
                        if (reactor != null)
                        {
                            _owner._functionalBlockManager.AddFunctionalBlock(reactor);
                            _result = BlockCollectionResult.Success;
                            return BackgroundOperationResult.Completed;
                        }
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
