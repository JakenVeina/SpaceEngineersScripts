using System;

using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public class GasTankCollectionHandler : IBlockCollectionHandler
        {
            public GasTankCollectionHandler(
                IGasTankManager gasTankManager,
                ILogger logger,
                IProgramSettingsProvider programSettingsProvider)
            {
                _gasTankManager = gasTankManager;
                _logger = logger;
                _programSettingsProvider = programSettingsProvider;

                _collectGasTankOperationPool = new ObjectPool<CollectGasTankOperation>(onFinished
                    => new CollectGasTankOperation(this, onFinished));
            }

            public void OnStarting()
                => _gasTankManager.ClearGasTanks();

            public IBackgroundOperation<BlockCollectionResult> MakeCollectBlockOperation(IMyTerminalBlock block)
            {
                var collectGasTankOperation = _collectGasTankOperationPool.Get();

                collectGasTankOperation.Block = block;

                return collectGasTankOperation;
            }

            public void OnCompleted()
                => _logger.AddLine($"Discovered {_gasTankManager.GasTanks.Count} batteries for management");

            private readonly IGasTankManager _gasTankManager;

            private readonly ILogger _logger;

            private readonly IProgramSettingsProvider _programSettingsProvider;

            private readonly ObjectPool<CollectGasTankOperation> _collectGasTankOperationPool;

            private class CollectGasTankOperation : IBackgroundOperation<BlockCollectionResult>, IDisposable
            {
                public CollectGasTankOperation(
                    GasTankCollectionHandler owner,
                    Action onDisposed)
                {
                    _owner = owner;
                    _onDisposed = onDisposed;
                }

                public IMyTerminalBlock Block;

                public BlockCollectionResult Result
                    => _result;

                public BackgroundOperationResult Execute(Action<IBackgroundOperation> subOperationScheduler)
                {
                    _result = BlockCollectionResult.Ignored;

                    if (!_owner._programSettingsProvider.Settings.IgnoreGasTanks)
                    {
                        var gasTank = Block as IMyGasTank;
                        if (gasTank != null)
                        {
                            _owner._gasTankManager.AddGasTank(gasTank);
                            _result = BlockCollectionResult.Success;
                        }
                    }

                    return BackgroundOperationResult.Completed;
                }

                public void Dispose()
                {
                    Block = null;
                    _onDisposed.Invoke();
                }

                private readonly GasTankCollectionHandler _owner;

                private readonly Action _onDisposed;

                private BlockCollectionResult _result;
            }
        }
    }
}
