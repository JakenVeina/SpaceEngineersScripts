namespace IngameScript
{
    partial class Program
    {
        public partial struct BlockCollectionResult
        {
            public static BlockCollectionResult Success
                => new BlockCollectionResult()
                {
                    _isSuccess = true,
                    _isSkipped = false
                };

            public static BlockCollectionResult Ignored
                => new BlockCollectionResult()
                {
                    _isSuccess = false,
                    _isSkipped = false
                };

            public static BlockCollectionResult Skipped
                => new BlockCollectionResult()
                {
                    _isSuccess = false,
                    _isSkipped = true
                };

            public bool IsSuccess
                => _isSuccess;

            public bool IsIgnored
                => !_isSuccess && !_isSkipped;

            public bool IsSkipped
                => _isSkipped;

            private bool _isSuccess;

            private bool _isSkipped;
        }
    }
}
