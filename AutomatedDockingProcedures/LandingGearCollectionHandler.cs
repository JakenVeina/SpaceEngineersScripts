using System;

using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public class LandingGearCollectionHandler : IBlockCollectionHandler
        {
            public LandingGearCollectionHandler(
                ILandingGearManager landingGearManager,
                ILogger logger)
            {
                _landingGearManager = landingGearManager;
                _logger = logger;

                _collectLandingGearOperationPool = new ObjectPool<CollectLandingGearOperation>(onFinished
                    => new CollectLandingGearOperation(this, onFinished));
            }

            public void OnStarting()
                => _landingGearManager.ClearLandingGears();

            public IBackgroundOperation<BlockCollectionResult> MakeCollectBlockOperation(IMyTerminalBlock block)
            {
                var collectLandingGearOperation = _collectLandingGearOperationPool.Get();

                collectLandingGearOperation.Block = block;

                return collectLandingGearOperation;
            }

            public void OnCompleted()
                => _logger.AddLine($"Discovered {_landingGearManager.LandingGears.Count} batteries for management");

            private readonly ILandingGearManager _landingGearManager;

            private readonly ILogger _logger;

            private readonly ObjectPool<CollectLandingGearOperation> _collectLandingGearOperationPool;

            private class CollectLandingGearOperation : IBackgroundOperation<BlockCollectionResult>, IDisposable
            {
                public CollectLandingGearOperation(
                    LandingGearCollectionHandler owner,
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

                    var landingGear = Block as IMyLandingGear;
                    if (landingGear != null)
                    {
                        _owner._landingGearManager.AddLandingGear(landingGear);
                        _result = BlockCollectionResult.Success;
                    }

                    return BackgroundOperationResult.Completed;
                }

                public void Dispose()
                {
                    Block = null;
                    _onDisposed.Invoke();
                }

                private readonly LandingGearCollectionHandler _owner;

                private readonly Action _onDisposed;

                private BlockCollectionResult _result;
            }
        }
    }
}
