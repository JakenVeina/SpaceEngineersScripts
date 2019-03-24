using System;

using SpaceEngineers.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public interface ILandingGearManager : IBlockManager<IMyLandingGear> { }

        public sealed class LandingGearManager : BlockManagerBase<IMyLandingGear>, ILandingGearManager
        {
            internal protected override OnDockOperationBase CreateOnDockingOperation(BlockManagerBase<IMyLandingGear> owner, Action onDisposed)
                => new LockOperation(owner, onDisposed);

            internal protected override OnDockOperationBase CreateOnUndockingOperation(BlockManagerBase<IMyLandingGear> owner, Action onDisposed)
                => new UnlockOperation(owner, onDisposed);

            private sealed class LockOperation : OnDockOperationBase
            {
                public LockOperation(BlockManagerBase<IMyLandingGear> owner, Action onDisposed)
                    : base(owner, onDisposed) { }

                protected override void OnExecuting(IMyLandingGear landingGear)
                    => landingGear.Lock();
            }

            private sealed class UnlockOperation : OnDockOperationBase
            {
                public UnlockOperation(BlockManagerBase<IMyLandingGear> owner, Action onDisposed)
                    : base(owner, onDisposed) { }

                protected override void OnExecuting(IMyLandingGear landingGear)
                    => landingGear.Unlock();
            }
        }
    }
}
