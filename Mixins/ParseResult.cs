namespace IngameScript
{
    public partial class Program
    {
        public partial struct ParseResult
        {
            public static ParseResult Success
                => new ParseResult()
                {
                    _isSuccess = true,
                    _error = null
                };

            public static ParseResult Ignored
                => new ParseResult()
                {
                    _isSuccess = false,
                    _error = null
                };

            public static ParseResult FromError(string error)
                => new ParseResult()
                {
                    _isSuccess = false,
                    _error = error
                };

            public bool IsSuccess
                => _isSuccess;

            public bool IsIgnored
                => !_isSuccess && (_error == null);

            public bool IsError
                => _error != null;

            public string Error
                => _error;

            private bool _isSuccess;

            private string _error;
        }
    }
}
