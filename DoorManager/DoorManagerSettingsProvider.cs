using System;
using System.Collections.Generic;

namespace IngameScript
{
    public partial class Program
    {
        public interface IDoorManagerSettingsProvider
        {
            DoorManagerSettings Settings { get; set; }
        }

        public class DoorManagerSettingsProvider : IDoorManagerSettingsProvider, IConfigParseHandler
        {
            public DoorManagerSettings Settings
            {
                get { return _settings; }
                set { _settings = value; }
            }

            public void OnStarting()
                => _settings = new DoorManagerSettings()
                {
                    AutoCloseInterval = TimeSpan.FromSeconds(3)
                };

            public ParseResult OnParsing(ConfigLine configLine)
            {
                switch (configLine.Option)
                {
                    case "auto-close-interval":
                        int autoCloseIntervalMilliseconds;
                        if ((configLine.ParamCount != 1)
                                || !int.TryParse(configLine.GetParam(0), out autoCloseIntervalMilliseconds)
                                || (autoCloseIntervalMilliseconds < 1))
                            return ParseResult.FromError("Usage: \"auto-close-interval:<AutoCloseInterval>\" (> 0) (ms)");
                        _settings.AutoCloseInterval = TimeSpan.FromMilliseconds(autoCloseIntervalMilliseconds);
                        return ParseResult.Success;

                    default:
                        return ParseResult.Ignored;
                }
            }

            public void OnCompleted() { }

            private DoorManagerSettings _settings;
        }
    }
}
