using System;

using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public interface IFunctionalBlockManager : IBlockManager<IMyFunctionalBlock>
        {
            int BeaconCount { get; }

            int GasGeneratorCount { get; }

            int GyroCount { get; }

            int LightingBlockCount { get; }

            int RadioAntennaCount { get; }

            int ReactorCount { get; }

            int ThrusterCount { get; }
        }

        public sealed class FunctionalBlockManager : BlockManagerBase<IMyFunctionalBlock>, IFunctionalBlockManager
        {
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

            public int ThrusterCount
                => _thrusterCount;

            public override void AddBlock(IMyFunctionalBlock block)
            {
                base.AddBlock(block);

                if (block is IMyBeacon)
                    ++_beaconCount;
                else if (block is IMyGasGenerator)
                    ++_gasGeneratorCount;
                else if (block is IMyGyro)
                    ++_gyroCount;
                else if (block is IMyLightingBlock)
                    ++_lightingBlockCount;
                else if (block is IMyRadioAntenna)
                    ++_radioAntennaCount;
                else if (block is IMyReactor)
                    ++_reactorCount;
                else if (block is IMyThrust)
                    ++_thrusterCount;
            }

            public override void ClearBlocks()
            {
                base.ClearBlocks();

                _beaconCount = 0;
                _gasGeneratorCount = 0;
                _gyroCount = 0;
                _lightingBlockCount = 0;
                _radioAntennaCount = 0;
                _reactorCount = 0;
                _thrusterCount = 0;
            }

            internal protected override OnDockOperationBase CreateOnDockingOperation(BlockManagerBase<IMyFunctionalBlock> owner, Action onDisposed)
                => new DisableOperation(owner, onDisposed);

            internal protected override OnDockOperationBase CreateOnUndockingOperation(BlockManagerBase<IMyFunctionalBlock> owner, Action onDisposed)
                => new EnableOperation(owner, onDisposed);

            private int _beaconCount;

            private int _gasGeneratorCount;

            private int _gyroCount;

            private int _lightingBlockCount;

            private int _radioAntennaCount;

            private int _reactorCount;

            private int _thrusterCount;

            private sealed class DisableOperation : OnDockOperationBase
            {
                public DisableOperation(BlockManagerBase<IMyFunctionalBlock> owner, Action onDisposed)
                    : base(owner, onDisposed) { }

                protected override void OnExecuting(IMyFunctionalBlock functionalBlock)
                    => functionalBlock.Enabled = false;
            }

            private sealed class EnableOperation : OnDockOperationBase
            {
                public EnableOperation(BlockManagerBase<IMyFunctionalBlock> owner, Action onDisposed)
                    : base(owner, onDisposed) { }

                protected override void OnExecuting(IMyFunctionalBlock functionalBlock)
                    => functionalBlock.Enabled = true;
            }
        }
    }
}
