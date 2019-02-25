using System;
using System.Collections.Generic;

using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public interface IBatteryManager
        {
            IReadOnlyList<IMyBatteryBlock> BatteryBlocks { get; }

            void AddBatteryBlock(IMyBatteryBlock batteryBlock);

            void ClearBatteryBlocks();

            IBackgroundOperation MakeRechargeOperation();

            IBackgroundOperation MakeDischargeOperation();
        }

        public class BatteryBlockManager : IBatteryManager
        {
            public BatteryBlockManager()
            {
                _rechargeOperationPool = new ObjectPool<RechargeOperation>(onFinished
                    => new RechargeOperation(this, onFinished));

                _dischargeOperationPool = new ObjectPool<DischargeOperation>(onFinished
                    => new DischargeOperation(this, onFinished));
            }

            public IReadOnlyList<IMyBatteryBlock> BatteryBlocks
                => _batteryBlocks;

            public void AddBatteryBlock(IMyBatteryBlock batteryBlock)
                => _batteryBlocks.Add(batteryBlock);

            public void ClearBatteryBlocks()
                => _batteryBlocks.Clear();

            public IBackgroundOperation MakeRechargeOperation()
                => _rechargeOperationPool.Get();

            public IBackgroundOperation MakeDischargeOperation()
                => _dischargeOperationPool.Get();

            private readonly List<IMyBatteryBlock> _batteryBlocks
                = new List<IMyBatteryBlock>();

            private readonly ObjectPool<RechargeOperation> _rechargeOperationPool;

            private readonly ObjectPool<DischargeOperation> _dischargeOperationPool;

            private abstract class BatteryOperationBase : IBackgroundOperation, IDisposable
            {
                public BatteryOperationBase(BatteryBlockManager owner, Action onDisposed)
                {
                    _owner = owner;
                    _onDisposed = onDisposed;

                    Reset();
                }

                public BackgroundOperationResult Execute(Action<IBackgroundOperation> subOperationScheduler)
                {
                    if (_owner._batteryBlocks.Count == 0)
                        return BackgroundOperationResult.Completed;

                    OnExecuting(_owner._batteryBlocks[_batteryBlockIndex]);

                    if (++_batteryBlockIndex < _owner._batteryBlocks.Count)
                        return BackgroundOperationResult.NotCompleted;

                    return BackgroundOperationResult.Completed;
                }

                public void Dispose()
                {
                    Reset();
                    _onDisposed.Invoke();
                }

                protected abstract void OnExecuting(IMyBatteryBlock batteryBlock);

                private void Reset()
                    => _batteryBlockIndex = 0;

                private readonly BatteryBlockManager _owner;

                private readonly Action _onDisposed;

                private int _batteryBlockIndex;
            }

            private sealed class RechargeOperation : BatteryOperationBase
            {
                public RechargeOperation(BatteryBlockManager owner, Action onDisposed)
                    : base(owner, onDisposed) { }

                protected override void OnExecuting(IMyBatteryBlock batteryBlock)
                {
                    batteryBlock.SemiautoEnabled = false;
                    batteryBlock.OnlyDischarge = false;
                    batteryBlock.OnlyRecharge = true;
                }
            }

            private sealed class DischargeOperation : BatteryOperationBase
            {
                public DischargeOperation(BatteryBlockManager owner, Action onDisposed)
                    : base(owner, onDisposed) { }

                protected override void OnExecuting(IMyBatteryBlock batteryBlock)
                {
                    batteryBlock.SemiautoEnabled = false;
                    batteryBlock.OnlyDischarge = true;
                    batteryBlock.OnlyRecharge = false;
                }
            }
        }
    }
}
