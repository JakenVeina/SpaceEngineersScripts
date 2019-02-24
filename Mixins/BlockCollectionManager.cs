using System;
using System.Collections.Generic;

using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public interface IBlockCollectionManager
        {
            IBackgroundOperation MakeCollectBlocksOperation();
        }

        public interface IBlockCollectionHandler
        {
            void OnStarting();

            IBackgroundOperation<BlockCollectionResult> MakeCollectBlockOperation(IMyTerminalBlock block);

            void OnCompleted();
        }

        public partial class BlockCollectionManager<T> : IBlockCollectionManager where T : class, IMyTerminalBlock
        {
            public BlockCollectionManager(
                IReadOnlyList<IBlockCollectionHandler> blockCollectionHandlers,
                IMyGridTerminalSystem gridTerminalSystem)
            {
                _blockCollectionHandlers = blockCollectionHandlers;
                _gridTerminalSystem = gridTerminalSystem;

                _collectBlocksOperationPool = new ObjectPool<CollectBlocksOperation>(onFinished
                    => new CollectBlocksOperation(this, onFinished));
            }

            public IBackgroundOperation MakeCollectBlocksOperation()
                => _collectBlocksOperationPool.Get();

            private readonly IReadOnlyList<IBlockCollectionHandler> _blockCollectionHandlers;

            private readonly IMyGridTerminalSystem _gridTerminalSystem;

            private readonly ObjectPool<CollectBlocksOperation> _collectBlocksOperationPool;

            private class CollectBlocksOperation : IBackgroundOperation, IDisposable
            {
                public CollectBlocksOperation(BlockCollectionManager<T> owner, Action onDisposed)
                {
                    _owner = owner;
                    _onDisposed = onDisposed;

                    Reset();
                }

                public BackgroundOperationResult Execute(Action<IBackgroundOperation> subOperationScheduler)
                    => _executeActionsByState[(int)_state].Invoke(this, subOperationScheduler);

                public void Dispose()
                {
                    Reset();
                    _onDisposed.Invoke();
                }

                private void Reset()
                    => _state = OperationState.Initializing;

                private static BackgroundOperationResult OnInitializing(CollectBlocksOperation @this, Action<IBackgroundOperation> subOperationScheduler)
                {
                    if (@this._owner._blockCollectionHandlers.Count == 0)
                        return BackgroundOperationResult.Completed;

                    @this._buffer.Clear();
                    @this._owner._gridTerminalSystem.GetBlocksOfType(@this._buffer);

                    @this._handlerIndex = 0;

                    @this._state = OperationState.Starting;
                    return BackgroundOperationResult.NotCompleted;
                }

                private static BackgroundOperationResult OnStarting(CollectBlocksOperation @this, Action<IBackgroundOperation> subOperationScheduler)
                {
                    @this._owner._blockCollectionHandlers[@this._handlerIndex].OnStarting();

                    if (++@this._handlerIndex >= @this._owner._blockCollectionHandlers.Count)
                    {
                        @this._handlerIndex = 0;
                        @this._bufferIndex = 0;
                        @this._previousCollectBlockOperation = null;
                        @this._state = (@this._buffer.Count == 0)
                            ? OperationState.Completing
                            : OperationState.Collecting;
                    }

                    return BackgroundOperationResult.NotCompleted;
                }

                private static BackgroundOperationResult OnCollecting(CollectBlocksOperation @this, Action<IBackgroundOperation> subOperationScheduler)
                {
                    if ((@this._previousCollectBlockOperation == null) || !@this._previousCollectBlockOperation.Result.IsSkipped)
                    {
                        @this._previousCollectBlockOperation = @this._owner._blockCollectionHandlers[@this._handlerIndex].MakeCollectBlockOperation(@this._buffer[@this._bufferIndex]);

                        subOperationScheduler.Invoke(@this._previousCollectBlockOperation);

                        if (++@this._handlerIndex < @this._owner._blockCollectionHandlers.Count)
                            return BackgroundOperationResult.NotCompleted;
                    }
                    @this._handlerIndex = 0;
                    @this._previousCollectBlockOperation = null;

                    if (++@this._bufferIndex >= @this._buffer.Count)
                        @this._state = OperationState.Completing;

                    return BackgroundOperationResult.NotCompleted;
                }

                private static BackgroundOperationResult OnCompleting(CollectBlocksOperation @this, Action<IBackgroundOperation> subOperationScheduler)
                {
                    @this._owner._blockCollectionHandlers[@this._handlerIndex].OnCompleted();

                    if (++@this._handlerIndex < @this._owner._blockCollectionHandlers.Count)
                        return BackgroundOperationResult.NotCompleted;

                    @this._buffer.Clear();
                    return BackgroundOperationResult.Completed;
                }

                private enum OperationState
                {
                    Initializing = 0,
                    Starting = 1,
                    Collecting = 2,
                    Completing = 3
                }

                private readonly BlockCollectionManager<T> _owner;

                private readonly Action _onDisposed;

                private readonly List<T> _buffer
                    = new List<T>();

                private OperationState _state;

                private int _handlerIndex;

                private int _bufferIndex;

                private IBackgroundOperation<BlockCollectionResult> _previousCollectBlockOperation;

                private static readonly Func<CollectBlocksOperation, Action<IBackgroundOperation>, BackgroundOperationResult>[] _executeActionsByState
                    = new Func<CollectBlocksOperation, Action<IBackgroundOperation>, BackgroundOperationResult>[]
                    {
                        OnInitializing,
                        OnStarting,
                        OnCollecting,
                        OnCompleting
                    };
            }
        }
    }
}
