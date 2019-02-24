using System;

namespace IngameScript
{
    partial class Program
    {
        public class ProgramReloadHandler : IConfigParseHandler
        {
            public ProgramReloadHandler(
                IBackgroundWorker backgroundWorker,
                IBlockCollectionManager blockCollectionManager,
                IDoorManager doorManager,
                IProgramSettingsProvider programSettingsProvider)
            {
                _backgroundWorker = backgroundWorker;
                _blockCollectionManager = blockCollectionManager;
                _programSettingsProvider = programSettingsProvider;

                _manageDoorsOperationConstructor = doorManager.MakeManageDoorsOperation;
            }

            public void OnStarting()
            {
                _backgroundWorker.ClearRecurringOperations();

                _backgroundWorker.ScheduleOperation(_blockCollectionManager.MakeCollectBlocksOperation());
            }

            public ParseResult OnParsing(ConfigLine configLine)
                => ParseResult.Ignored;

            public void OnCompleted()
                => _backgroundWorker.RegisterRecurringOperation(_programSettingsProvider.Settings.ManageInterval, _manageDoorsOperationConstructor);

            private readonly IBackgroundWorker _backgroundWorker;

            private readonly IBlockCollectionManager _blockCollectionManager;

            private readonly IProgramSettingsProvider _programSettingsProvider;

            private readonly Func<IBackgroundOperation> _manageDoorsOperationConstructor;
        }
    }
}
