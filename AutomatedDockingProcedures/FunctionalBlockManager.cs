using System;
using System.Collections.Generic;

using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public interface IFunctionalBlockManager
        {
            IReadOnlyList<IMyFunctionalBlock> FunctionalBlocks { get; }

            int BeaconCount { get; }

            int GasGeneratorCount { get; }

            int GyroCount { get; }

            int LightingBlockCount { get; }

            int RadioAntennaCount { get; }

            int ReactorCount { get; }

            void AddFunctionalBlock(IMyFunctionalBlock functionalBlock);

            void ClearFunctionalBlocks();

            IBackgroundOperation MakeDisableOperation();

            IBackgroundOperation MakeEnableOperation();
        }

        public class FunctionalBlockManager : IFunctionalBlockManager
        {
            public FunctionalBlockManager()
            {
                _disableOperationPool = new ObjectPool<DisableOperation>(onFinished
                    => new DisableOperation(this, onFinished));

                _enableOperationPool = new ObjectPool<EnableOperation>(onFinished
                    => new EnableOperation(this, onFinished));
            }

            public IReadOnlyList<IMyFunctionalBlock> FunctionalBlocks
                => _functionalBlocks;

            public int BeaconCount
                => _beaconCount;

            public int GasGeneratorCount
                => _gasGeneratorCount;

            public int GyroCount
                => _gyroCount;

            public int LightingBlockCount
                => _lightingBlockCount;

            public int RadioAntennaCount
                => _radioAntennaCount;

            public int ReactorCount
                => _reactorCount;

            public void AddFunctionalBlock(IMyFunctionalBlock functionalBlock)
            {
                _functionalBlocks.Add(functionalBlock);

                if (functionalBlock is IMyBeacon)
                    ++_beaconCount;
                else if (functionalBlock is IMyGasGenerator)
                    ++_gasGeneratorCount;
                else if (functionalBlock is IMyGyro)
                    ++_gyroCount;
                else if (functionalBlock is IMyLightingBlock)
                    ++_lightingBlockCount;
                else if (functionalBlock is IMyRadioAntenna)
                    ++_radioAntennaCount;
                else if (functionalBlock is IMyReactor)
                    ++_reactorCount;
            }

            public void ClearFunctionalBlocks()
            {
                _functionalBlocks.Clear();

                _beaconCount = 0;
                _gasGeneratorCount = 0;
                _gyroCount = 0;
                _lightingBlockCount = 0;
                _radioAntennaCount = 0;
                _reactorCount = 0;
            }

            public IBackgroundOperation MakeDisableOperation()
                => _disableOperationPool.Get();

            public IBackgroundOperation MakeEnableOperation()
                => _enableOperationPool.Get();

            private readonly List<IMyFunctionalBlock> _functionalBlocks
                = new List<IMyFunctionalBlock>();

            private readonly ObjectPool<DisableOperation> _disableOperationPool;

            private readonly ObjectPool<EnableOperation> _enableOperationPool;

            private int _beaconCount;

            private int _gasGeneratorCount;

            private int _gyroCount;

            private int _lightingBlockCount;

            private int _radioAntennaCount;

            private int _reactorCount;

            private abstract class FunctionalBlockOperationBase : IBackgroundOperation, IDisposable
            {
                public FunctionalBlockOperationBase(FunctionalBlockManager owner, Action onDisposed)
                {
                    _owner = owner;
                    _onDisposed = onDisposed;

                    Reset();
                }

                public BackgroundOperationResult Execute(Action<IBackgroundOperation> subOperationScheduler)
                {
                    if (_owner._functionalBlocks.Count == 0)
                        return BackgroundOperationResult.Completed;

                    OnExecuting(_owner._functionalBlocks[_functionalBlockIndex]);

                    if (++_functionalBlockIndex < _owner._functionalBlocks.Count)
                        return BackgroundOperationResult.NotCompleted;

                    return BackgroundOperationResult.Completed;
                }

                public void Dispose()
                {
                    Reset();
                    _onDisposed.Invoke();
                }

                protected abstract void OnExecuting(IMyFunctionalBlock functionalBlock);

                private void Reset()
                    => _functionalBlockIndex = 0;

                private readonly FunctionalBlockManager _owner;

                private readonly Action _onDisposed;

                private int _functionalBlockIndex;
            }

            private sealed class DisableOperation : FunctionalBlockOperationBase
            {
                public DisableOperation(FunctionalBlockManager owner, Action onDisposed)
                    : base(owner, onDisposed) { }

                protected override void OnExecuting(IMyFunctionalBlock functionalBlock)
                    => functionalBlock.Enabled = false;
            }

            private sealed class EnableOperation : FunctionalBlockOperationBase
            {
                public EnableOperation(FunctionalBlockManager owner, Action onDisposed)
                    : base(owner, onDisposed) { }

                protected override void OnExecuting(IMyFunctionalBlock functionalBlock)
                    => functionalBlock.Enabled = true;
            }
        }
    }
}
