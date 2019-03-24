using System;

using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public interface IGasTankManager : IBlockManager<IMyGasTank> { }

        public sealed class GasTankManager : BlockManagerBase<IMyGasTank>, IGasTankManager
        {
            internal protected override OnDockOperationBase CreateOnDockingOperation(BlockManagerBase<IMyGasTank> owner, Action onDisposed)
                => new StockpileOperation(owner, onDisposed);

            internal protected override OnDockOperationBase CreateOnUndockingOperation(BlockManagerBase<IMyGasTank> owner, Action onDisposed)
                => new DispenseOperation(owner, onDisposed);

            private sealed class StockpileOperation : OnDockOperationBase
            {
                public StockpileOperation(BlockManagerBase<IMyGasTank> owner, Action onDisposed)
                    : base(owner, onDisposed) { }

                protected override void OnExecuting(IMyGasTank gasTank)
                    => gasTank.Stockpile = true;
            }

            private sealed class DispenseOperation : OnDockOperationBase
            {
                public DispenseOperation(BlockManagerBase<IMyGasTank> owner, Action onDisposed)
                    : base(owner, onDisposed) { }

                protected override void OnExecuting(IMyGasTank gasTank)
                    => gasTank.Stockpile = false;
            }
        }
    }
}
