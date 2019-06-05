using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public interface ILoggerSettingsProvider
        {
            LoggerSettings Settings { get; set; }
        }

        public partial class LoggerSettingsProvider : ILoggerSettingsProvider, IConfigParseHandler
        {
            public LoggerSettings Settings
            {
                get { return _settings; }
                set { _settings = value; }
            }

            public void OnStarting()
                => _settings = new LoggerSettings()
                {
                    MaxLogSize = 10
                };

            public ParseResult OnParsing(ConfigLine configLine)
            {
                switch (configLine.Option)
                {
                    case "max-log-size":
                        int maxLogSize;
                        if ((configLine.ParamCount != 1) 
                                || !int.TryParse(configLine.GetParam(0), out maxLogSize)
                                || (maxLogSize < 0))
                            return ParseResult.FromError("Usage: \"max-log-size:<MaxLogSize>\" (>= 0) (lines)");
                        _settings.MaxLogSize = maxLogSize;
                        return ParseResult.Success;

                    default:
                        return ParseResult.Ignored;
                }
            }

            public void OnCompleted() { }

            private LoggerSettings _settings;
        }
    }
}
