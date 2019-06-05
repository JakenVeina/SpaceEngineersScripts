using System;

namespace IngameScript
{
    public partial class Program
    {
        public interface IManagedDoorSettingsProvider
        {
            ManagedDoorSettings Settings { get; set; }
        }

        public class ManagedDoorSettingsProvider : IManagedDoorSettingsProvider, IManagedBlockConfigParseHandler
        {
            public ManagedDoorSettingsProvider(
                IDoorManagerSettingsProvider doorManagerSettingsProvider)
            {
                _doorManagerSettingsProvider = doorManagerSettingsProvider;
            }

            public ManagedDoorSettings Settings
            {
                get { return _settings; }
                set { _settings = value; }
            }

            public void OnStarting()
                => _settings = new ManagedDoorSettings()
                {
                    AutoCloseInterval = _doorManagerSettingsProvider.Settings.AutoCloseInterval
                };

            public ParseResult OnParsing(ManagedBlockConfigLine configLine)
            {
                switch (configLine.Option)
                {
                    case "auto-close-interval":
                        int autoCloseIntervalMilliseconds;
                        if ((configLine.ParamCount != 1)
                                || !int.TryParse(configLine.GetParam(0), out autoCloseIntervalMilliseconds)
                                || (autoCloseIntervalMilliseconds < 1))
                            return ParseResult.FromError("Usage: \"<BlockTag>:auto-close-interval:<AutoCloseInterval>\" (> 0) (ms)");
                        _settings.AutoCloseInterval = TimeSpan.FromMilliseconds(autoCloseIntervalMilliseconds);
                        return ParseResult.Success;

                    default:
                        return ParseResult.Ignored;
                }
            }

            public void OnCompleted() { }

            private readonly IDoorManagerSettingsProvider _doorManagerSettingsProvider;

            private ManagedDoorSettings _settings;
        }
    }
}
