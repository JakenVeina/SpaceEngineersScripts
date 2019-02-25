using System;
using System.Collections.Generic;

using SpaceEngineers.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public interface ILandingGearManager
        {
            IReadOnlyList<IMyLandingGear> LandingGears { get; }

            void AddLandingGear(IMyLandingGear landingGear);

            void ClearLandingGears();

            IBackgroundOperation MakeLockOperation();

            IBackgroundOperation MakeUnlockOperation();
        }

        public class LandingGearManager : ILandingGearManager
        {
            public LandingGearManager()
            {
                _lockOperationPool = new ObjectPool<LockOperation>(onFinished
                    => new LockOperation(this, onFinished));

                _unlockOperationPool = new ObjectPool<UnlockOperation>(onFinished
                    => new UnlockOperation(this, onFinished));
            }

            public IReadOnlyList<IMyLandingGear> LandingGears
                => _landingGears;

            public void AddLandingGear(IMyLandingGear landingGear)
                => _landingGears.Add(landingGear);

            public void ClearLandingGears()
                => _landingGears.Clear();

            public IBackgroundOperation MakeLockOperation()
                => _lockOperationPool.Get();

            public IBackgroundOperation MakeUnlockOperation()
                => _unlockOperationPool.Get();

            private readonly List<IMyLandingGear> _landingGears
                = new List<IMyLandingGear>();

            private readonly ObjectPool<LockOperation> _lockOperationPool;

            private readonly ObjectPool<UnlockOperation> _unlockOperationPool;

            private abstract class LandingGearOperationBase : IBackgroundOperation, IDisposable
            {
                public LandingGearOperationBase(LandingGearManager owner, Action onDisposed)
                {
                    _owner = owner;
                    _onDisposed = onDisposed;

                    Reset();
                }

                public BackgroundOperationResult Execute(Action<IBackgroundOperation> subOperationScheduler)
                {
                    if (_owner._landingGears.Count == 0)
                        return BackgroundOperationResult.Completed;

                    OnExecuting(_owner._landingGears[_landingGearIndex]);

                    if (++_landingGearIndex < _owner._landingGears.Count)
                        return BackgroundOperationResult.NotCompleted;

                    return BackgroundOperationResult.Completed;
                }

                public void Dispose()
                {
                    Reset();
                    _onDisposed.Invoke();
                }

                protected abstract void OnExecuting(IMyLandingGear connector);

                private void Reset()
                    => _landingGearIndex = 0;

                private readonly LandingGearManager _owner;

                private readonly Action _onDisposed;

                private int _landingGearIndex;
            }

            private sealed class LockOperation : LandingGearOperationBase
            {
                public LockOperation(LandingGearManager owner, Action onDisposed)
                    : base(owner, onDisposed) { }

                protected override void OnExecuting(IMyLandingGear connector)
                    => connector.Lock();
            }

            private sealed class UnlockOperation : LandingGearOperationBase
            {
                public UnlockOperation(LandingGearManager owner, Action onDisposed)
                    : base(owner, onDisposed) { }

                protected override void OnExecuting(IMyLandingGear connector)
                    => connector.Unlock();
            }
        }
    }
}
