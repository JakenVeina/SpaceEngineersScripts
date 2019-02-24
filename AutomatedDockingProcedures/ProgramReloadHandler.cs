using System;

namespace IngameScript
{
    partial class Program
    {
        public class ProgramReloadHandler : IConfigParseHandler
        {
            public ProgramReloadHandler(
                IBackgroundWorker backgroundWorker,
                IBlockCollectionManager blockCollectionManager)
            {
                _backgroundWorker = backgroundWorker;
                _blockCollectionManager = blockCollectionManager;
            }

            public void OnStarting()
            {
                _backgroundWorker.ScheduleOperation(_blockCollectionManager.MakeCollectBlocksOperation());

                throw new Exception("Not yet implemented");
            }

            public ParseResult OnParsing(ConfigLine configLine)
                => ParseResult.Ignored;

            public void OnCompleted()
            {
                throw new Exception("Not yet implemented");
            }

            private readonly IBackgroundWorker _backgroundWorker;

            private readonly IBlockCollectionManager _blockCollectionManager;
        }
    }
}
