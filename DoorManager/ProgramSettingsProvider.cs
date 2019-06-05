using System;

namespace IngameScript
{
    public partial class Program
    {
        public interface IProgramSettingsProvider
        {
            ProgramSettings Settings { get; set; }
        }

        public class ProgramSettingsProvider : IProgramSettingsProvider, IConfigParseHandler
        {
            public ProgramSettings Settings
            {
                get { return _settings; }
                set { _settings = value; }
            }

            public void OnStarting()
                => _settings = new ProgramSettings()
                {
                    ManageInterval = TimeSpan.FromMilliseconds(500)
                };

            public ParseResult OnParsing(ConfigLine configLine)
            {
                switch (configLine.Option)
                {
                    case "manage-interval":
                        int manageIntervalMilliseconds;
                        if ((configLine.ParamCount != 1)
                                || !int.TryParse(configLine.GetParam(0), out manageIntervalMilliseconds)
                                || (manageIntervalMilliseconds < 1))
                            return ParseResult.FromError("Usage: \"manage-interval:<ManageInterval>\" (> 0) (ms)");
                        _settings.ManageInterval = TimeSpan.FromMilliseconds(manageIntervalMilliseconds);
                        return ParseResult.Success;

                    default:
                        return ParseResult.Ignored;
                }
            }

            public void OnCompleted() { }

            private ProgramSettings _settings;
        }
    }
}
