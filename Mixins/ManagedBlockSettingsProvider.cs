using System.Collections.Generic;

namespace IngameScript
{
    public partial class Program
    {
        public interface IManagedBlockSettingsProvider
        {
            ManagedBlockSettings Settings { get; set; }
        }

        public partial class ManagedBlockSettingsProvider : IManagedBlockSettingsProvider, IManagedBlockConfigParseHandler
        {
            public ManagedBlockSettings Settings
            {
                get { return _settings; }
                set { _settings = value; }
            }

            public void OnStarting()
                => _settings = new ManagedBlockSettings()
                {
                    Ignore = false,
                    Manage = false
                };

            public ParseResult OnParsing(ManagedBlockConfigLine configLine)
            {
                switch (configLine.Option)
                {
                    case "manage":
                        if (configLine.ParamCount != 0)
                            return ParseResult.FromError("Usage: \"[<BlockTag>]:manage\"");
                        _settings.Manage = true;
                        return ParseResult.Success;

                    case "ignore":
                        if (configLine.ParamCount != 0)
                            return ParseResult.FromError("Usage: \"[<BlockTag>]:ignore\"");
                        _settings.Ignore = true;
                        return ParseResult.Success;

                    default:
                        _settings.Manage = true;
                        return ParseResult.Ignored;
                }
            }

            public void OnCompleted() { }

            private ManagedBlockSettings _settings;
        }
    }
}
