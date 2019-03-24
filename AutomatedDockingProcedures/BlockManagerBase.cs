using System;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public interface IBlockManager<TBlock>
        {
            IReadOnlyList<TBlock> Blocks { get; }

            void AddBlock(TBlock block);

            void ClearBlocks();

            IBackgroundOperation MakeOnDockingOperation();

            IBackgroundOperation MakeOnUndockingOperation();
        }

        public abstract class BlockManagerBase<TBlock> : IBlockManager<TBlock>
        {
            protected BlockManagerBase()
            {
                _onDockingOperationPool = new ObjectPool<OnDockOperationBase>(onFinished => 
                    CreateOnDockingOperation(this, onFinished));

                _onUndockingOperationPool = new ObjectPool<OnDockOperationBase>(onFinished => 
                    CreateOnUndockingOperation(this, onFinished));
            }

            public IReadOnlyList<TBlock> Blocks
                => _blocks;

            public virtual void AddBlock(TBlock block)
                => _blocks.Add(block);

            public virtual void ClearBlocks()
                => _blocks.Clear();

            public IBackgroundOperation MakeOnDockingOperation()
                => _onDockingOperationPool.Get();

            public IBackgroundOperation MakeOnUndockingOperation()
                => _onUndockingOperationPool.Get();

            internal protected abstract OnDockOperationBase CreateOnDockingOperation(BlockManagerBase<TBlock> owner, Action onDisposed);

            internal protected abstract OnDockOperationBase CreateOnUndockingOperation(BlockManagerBase<TBlock> owner, Action onDisposed);

            private readonly List<TBlock> _blocks
                = new List<TBlock>();

            private readonly ObjectPool<OnDockOperationBase> _onDockingOperationPool;

            private readonly ObjectPool<OnDockOperationBase> _onUndockingOperationPool;

            internal protected abstract class OnDockOperationBase : IBackgroundOperation, IDisposable
            {
                public OnDockOperationBase(BlockManagerBase<TBlock> owner, Action onDisposed)
                {
                    _owner = owner;
                    _onDisposed = onDisposed;

                    Reset();
                }

                public BackgroundOperationResult Execute(Action<IBackgroundOperation> subOperationScheduler)
                {
                    if (_owner.Blocks.Count == 0)
                        return BackgroundOperationResult.Completed;

                    OnExecuting(_owner.Blocks[_blockIndex]);

                    if (++_blockIndex < _owner.Blocks.Count)
                        return BackgroundOperationResult.NotCompleted;

                    return BackgroundOperationResult.Completed;
                }

                public void Dispose()
                {
                    Reset();
                    _onDisposed.Invoke();
                }

                protected abstract void OnExecuting(TBlock block);

                private void Reset()
                    => _blockIndex = 0;

                private readonly BlockManagerBase<TBlock> _owner;

                private readonly Action _onDisposed;

                private int _blockIndex;
            }
        }
    }
}
