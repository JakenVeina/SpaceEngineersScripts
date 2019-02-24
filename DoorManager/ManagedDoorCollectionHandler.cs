using System;

using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public class ManagedDoorCollectionHandler : IBlockCollectionHandler
        {
            public ManagedDoorCollectionHandler(
                IDoorManager doorManager,
                ILogger logger,
                IManagedDoorSettingsProvider managedDoorSettingsProvider)
            {
                _doorManager = doorManager;
                _logger = logger;
                _managedDoorSettingsProvider = managedDoorSettingsProvider;

                _collectDoorOperationPool = new ObjectPool<CollectDoorOperation>(onFinished
                    => new CollectDoorOperation(this, onFinished));
            }

            public void OnStarting()
            {
                _doorManager.ClearDoors();
            }

            public IBackgroundOperation<BlockCollectionResult> MakeCollectBlockOperation(IMyTerminalBlock block)
            {
                var collectDoorOperation = _collectDoorOperationPool.Get();

                collectDoorOperation.Block = block;

                return collectDoorOperation;
            }

            public void OnCompleted()
                => _logger.AddLine($"Discovered {_doorManager.DoorCount} doors for management.");

            private readonly IDoorManager _doorManager;

            private readonly ILogger _logger;

            private readonly IManagedDoorSettingsProvider _managedDoorSettingsProvider;

            private readonly ObjectPool<CollectDoorOperation> _collectDoorOperationPool;

            private class CollectDoorOperation : IBackgroundOperation<BlockCollectionResult>, IDisposable
            {
                public CollectDoorOperation(ManagedDoorCollectionHandler owner, Action onDisposed)
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

                    var door = Block as IMyDoor;
                    if (door != null)
                    {
                        _owner._doorManager.AddDoor(door, _owner._managedDoorSettingsProvider.Settings);
                        _result = BlockCollectionResult.Success;
                    }

                    return BackgroundOperationResult.Completed;
                }

                public void Dispose()
                    => _onDisposed.Invoke();

                private readonly ManagedDoorCollectionHandler _owner;

                private readonly Action _onDisposed;

                private BlockCollectionResult _result;
            }
        }
    }
}
