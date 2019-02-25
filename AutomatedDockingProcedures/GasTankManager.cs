using System;
using System.Collections.Generic;

using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public interface IGasTankManager
        {
            IReadOnlyList<IMyGasTank> GasTanks { get; }

            void AddGasTank(IMyGasTank gasTank);

            void ClearGasTanks();

            IBackgroundOperation MakeStockpileOperation();

            IBackgroundOperation MakeDispenseOperation();
        }

        public class GasTankManager : IGasTankManager
        {
            public GasTankManager()
            {
                _stockpileOperationPool = new ObjectPool<StockpileOperation>(onFinished
                    => new StockpileOperation(this, onFinished));

                _dispenseOperationPool = new ObjectPool<DispenseOperation>(onFinished
                    => new DispenseOperation(this, onFinished));
            }

            public IReadOnlyList<IMyGasTank> GasTanks
                => _gasTanks;

            public void AddGasTank(IMyGasTank gasTank)
                => _gasTanks.Add(gasTank);

            public void ClearGasTanks()
                => _gasTanks.Clear();

            public IBackgroundOperation MakeStockpileOperation()
                => _stockpileOperationPool.Get();

            public IBackgroundOperation MakeDispenseOperation()
                => _dispenseOperationPool.Get();

            private readonly List<IMyGasTank> _gasTanks
                = new List<IMyGasTank>();

            private readonly ObjectPool<StockpileOperation> _stockpileOperationPool;

            private readonly ObjectPool<DispenseOperation> _dispenseOperationPool;

            private abstract class GasTankOperationBase : IBackgroundOperation, IDisposable
            {
                public GasTankOperationBase(GasTankManager owner, Action onDisposed)
                {
                    _owner = owner;
                    _onDisposed = onDisposed;

                    Reset();
                }

                public BackgroundOperationResult Execute(Action<IBackgroundOperation> subOperationScheduler)
                {
                    if (_owner._gasTanks.Count == 0)
                        return BackgroundOperationResult.Completed;

                    OnExecuting(_owner._gasTanks[_gasTankIndex]);

                    if (++_gasTankIndex < _owner._gasTanks.Count)
                        return BackgroundOperationResult.NotCompleted;

                    return BackgroundOperationResult.Completed;
                }

                public void Dispose()
                {
                    Reset();
                    _onDisposed.Invoke();
                }

                protected abstract void OnExecuting(IMyGasTank gasTank);

                private void Reset()
                    => _gasTankIndex = 0;

                private readonly GasTankManager _owner;

                private readonly Action _onDisposed;

                private int _gasTankIndex;
            }

            private sealed class StockpileOperation : GasTankOperationBase
            {
                public StockpileOperation(GasTankManager owner, Action onDisposed)
                    : base(owner, onDisposed) { }

                protected override void OnExecuting(IMyGasTank gasTank)
                    => gasTank.Stockpile = true;
            }

            private sealed class DispenseOperation : GasTankOperationBase
            {
                public DispenseOperation(GasTankManager owner, Action onDisposed)
                    : base(owner, onDisposed) { }

                protected override void OnExecuting(IMyGasTank gasTank)
                    => gasTank.Stockpile = false;
            }
        }
    }
}
