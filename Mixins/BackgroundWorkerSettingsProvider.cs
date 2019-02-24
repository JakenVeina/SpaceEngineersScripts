using System.Collections.Generic;

namespace IngameScript
{
    public partial class Program
    {
        public interface IBackgroundWorkerSettingsProvider
        {
            BackgroundWorkerSettings Settings { get; set; }
        }

        public partial class BackgroundWorkerSettingsProvider : IBackgroundWorkerSettingsProvider, IConfigParseHandler
        {
            public BackgroundWorkerSettingsProvider()
            {
                // Most config values we can leave uninitialized, and they'll be loaded from the first config parse,
                // but if we don't initialize this to something non-zero, the config parse will never be able to run.
                _settings = new BackgroundWorkerSettings()
                {
                    InstructionsPerExecution = 1000
                };
            }

            public BackgroundWorkerSettings Settings
            {
                get { return _settings; }
                set { _settings = value; }
            }

            public void OnStarting()
                => _settings = new BackgroundWorkerSettings()
                {
                    InstructionsPerExecution = 1000
                };

            public ParseResult OnParsing(ConfigLine configLine)
            {
                switch (configLine.Option)
                {
                    case "instructions-per-execution":
                        int instructionsPerExecution;
                        if ((configLine.ParamCount != 1) 
                                || !int.TryParse(configLine.GetParam(0), out instructionsPerExecution)
                                || (instructionsPerExecution < 1))
                            return ParseResult.FromError("Usage: \"instructions-per-execution:[InstructionsPerExecution (> 0)]\"");
                        _settings.InstructionsPerExecution = instructionsPerExecution;
                        return ParseResult.Success;

                    default:
                        return ParseResult.Ignored;
                }
            }

            public void OnCompleted() { }

            private BackgroundWorkerSettings _settings;
        }
    }
}
