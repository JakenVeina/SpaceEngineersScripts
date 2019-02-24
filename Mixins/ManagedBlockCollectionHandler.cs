using System;

using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public partial class ManagedBlockCollectionHandler : IBlockCollectionHandler
        {
            public ManagedBlockCollectionHandler(
                IManagedBlockConfigManager managedBlockConfigManager,
                IManagedBlockSettingsProvider managedBlockSettingsProvider,
                IManagerSettingsProvider managerSettingsProvider,
                IMyProgrammableBlock programmableBlock)
            {
                _managedBlockConfigManager = managedBlockConfigManager;
                _managedBlockSettingsProvider = managedBlockSettingsProvider;
                _managerSettingsProvider = managerSettingsProvider;
                _programmableBlock = programmableBlock;

                _collectBlockOperationPool = new ObjectPool<CollectBlockOperation>(onFinished
                    => new CollectBlockOperation(this, onFinished));
            }

            public void OnStarting() { }

            public IBackgroundOperation<BlockCollectionResult> MakeCollectBlockOperation(IMyTerminalBlock block)
            {
                var collectBlockOperation = _collectBlockOperationPool.Get();

                collectBlockOperation.Block = block;

                return collectBlockOperation;
            }

            public void OnCompleted() { }

            private readonly IManagedBlockConfigManager _managedBlockConfigManager;

            private readonly IManagedBlockSettingsProvider _managedBlockSettingsProvider;

            private readonly IManagerSettingsProvider _managerSettingsProvider;

            private readonly IMyProgrammableBlock _programmableBlock;

            private readonly ObjectPool<CollectBlockOperation> _collectBlockOperationPool;

            private class CollectBlockOperation : IBackgroundOperation<BlockCollectionResult>, IDisposable
            {
                public CollectBlockOperation(ManagedBlockCollectionHandler owner, Action onDisposed)
                {
                    _owner = owner;
                    _onDisposed = onDisposed;

                    Reset();
                }

                public IMyTerminalBlock Block;

                public BlockCollectionResult Result
                    => _result;

                public BackgroundOperationResult Execute(Action<IBackgroundOperation> subOperationScheduler)
                {
                    if(!_hasInitialized)
                    {
                        _result = BlockCollectionResult.Ignored;

                        if (!_owner._managerSettingsProvider.Settings.ManageOtherGrids && (Block.CubeGrid.EntityId != _owner._programmableBlock.CubeGrid.EntityId))
                            return Complete();

                        _hasParseBeenHandled = false;

                        _hasInitialized = true;
                    }

                    if(!_hasParseBeenHandled)
                    {
                        subOperationScheduler.Invoke(_owner._managedBlockConfigManager.MakeParseOperation(Block));

                        _hasParseBeenHandled = true;
                        return BackgroundOperationResult.NotCompleted;
                    }

                    if(!BlockShouldBeManaged())
                        _result = BlockCollectionResult.Skipped;

                    return Complete();
                }

                public void Dispose()
                {
                    Reset();
                    _onDisposed.Invoke();
                }

                private void Reset()
                    => _hasInitialized = false;

                private bool BlockShouldBeManaged()
                {
                    if (_owner._managedBlockSettingsProvider.Settings.Ignore)
                        return false;

                    if (_owner._managedBlockSettingsProvider.Settings.Manage)
                        return true;

                    if (!_owner._managerSettingsProvider.Settings.AutoManageThisGrid && !_owner._managerSettingsProvider.Settings.AutoManageOtherGrids)
                        return false;

                    var isSameGrid = Block.CubeGrid.EntityId == _owner._programmableBlock.CubeGrid.EntityId;

                    return (isSameGrid && _owner._managerSettingsProvider.Settings.AutoManageThisGrid)
                        || (!isSameGrid && _owner._managerSettingsProvider.Settings.AutoManageOtherGrids);
                }

                private BackgroundOperationResult Complete()
                {
                    Block = null;
                    return BackgroundOperationResult.Completed;
                }

                private readonly ManagedBlockCollectionHandler _owner;

                private readonly Action _onDisposed;

                private BlockCollectionResult _result;

                private bool _hasInitialized;

                private bool _hasParseBeenHandled;
            }
        }
    }
}
