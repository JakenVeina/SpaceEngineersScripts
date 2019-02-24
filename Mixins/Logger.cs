using System.Collections.Generic;

namespace IngameScript
{
    public partial class Program
    {
        public interface ILogger
        {
            void AddLine(string line);

            void Clear();

            string Render();
        }

        public partial class Logger : ILogger
        {
            public const string DateTimeFormat
                = "yyyy/MM/dd HH:mm:ss";

            public Logger(
                IDateTimeProvider dateTimeProvider,
                ILoggerSettingsProvider loggerSettingsProvider)
            {
                _dateTimeProvider = dateTimeProvider;
                _loggerSettingsProvider = loggerSettingsProvider;
            }

            public void AddLine(string line)
            {
                if (_loggerSettingsProvider.Settings.MaxLogSize == 0)
                    return;

                while (_logLines.Count >= _loggerSettingsProvider.Settings.MaxLogSize)
                    _logLines.RemoveFromBack();

                _logLines.AddToFront(line);
            }

            public void Clear()
                => _logLines.Clear();

            public string Render()
                => _dateTimeProvider.Now.ToString(DateTimeFormat)
                    + "\n\n"
                    + string.Join("\n", _logLines);

            internal protected IReadOnlyList<string> LogLines
                => _logLines;

            private readonly IDateTimeProvider _dateTimeProvider;

            private readonly ILoggerSettingsProvider _loggerSettingsProvider;

            private readonly Deque<string> _logLines
                = new Deque<string>(10);
        }
    }
}
