namespace IngameScript
{
    public partial class Program
    {
        public partial struct BackgroundOperationResult
        {
            public static readonly BackgroundOperationResult Completed
                = new BackgroundOperationResult(true);

            public static readonly BackgroundOperationResult NotCompleted
                = new BackgroundOperationResult(false);

            public BackgroundOperationResult(bool isComplete)
            {
                _isComplete = isComplete;
            }

            public bool IsComplete
                => _isComplete;

            public bool IsIncomplete
                => !_isComplete;

            private readonly bool _isComplete;
        }
    }
}
