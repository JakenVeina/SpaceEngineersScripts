using System;

using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public interface IBatteryBlockManager : IBlockManager<IMyBatteryBlock> { }

        public sealed class BatteryBlockManager : BlockManagerBase<IMyBatteryBlock>, IBatteryBlockManager
        {
            internal protected override OnDockOperationBase CreateOnDockingOperation(BlockManagerBase<IMyBatteryBlock> owner, Action onDisposed)
                => new RechargeOperation(owner, onDisposed);

            internal protected override OnDockOperationBase CreateOnUndockingOperation(BlockManagerBase<IMyBatteryBlock> owner, Action onDisposed)
                => new DischargeOperation(owner, onDisposed);

            private sealed class RechargeOperation : OnDockOperationBase
            {
                public RechargeOperation(BlockManagerBase<IMyBatteryBlock> owner, Action onDisposed)
                    : base(owner, onDisposed) { }

                protected override void OnExecuting(IMyBatteryBlock batteryBlock)
                    => batteryBlock.ChargeMode = ChargeMode.Recharge;
            }

            private sealed class DischargeOperation : OnDockOperationBase
            {
                public DischargeOperation(BlockManagerBase<IMyBatteryBlock> owner, Action onDisposed)
                    : base(owner, onDisposed) { }

                protected override void OnExecuting(IMyBatteryBlock batteryBlock)
                    => batteryBlock.ChargeMode = ChargeMode.Discharge;
            }
        }
    }
}
