using System.Collections.Generic;

namespace IngameScript
{
    public partial class Program
    {
        public interface IManagerSettingsProvider
        {
            ManagerSettings Settings { get; set; }
        }

        public partial class ManagerSettingsProvider : IManagerSettingsProvider, IConfigParseHandler
        {
            public string DefaultBlockTag;

            public ManagerSettings Settings
            {
                get { return _settings; }
                set { _settings = value; }
            }

            public void OnStarting()
                => _settings = new ManagerSettings()
                {
                    BlockTag = DefaultBlockTag,
                    ManageOtherGrids = false,
                    AutoManageThisGrid = false,
                    AutoManageOtherGrids = false
                };

            public ParseResult OnParsing(ConfigLine configLine)
            {
                switch (configLine.Option)
                {
                    case "block-tag":
                        if (configLine.ParamCount != 1)
                            return ParseResult.FromError("Usage: \"block-tag:[BlockTag]\"");
                        _settings.BlockTag = configLine.GetParam(0);
                        return ParseResult.Success;

                    case "manage-other-grids":
                        if (configLine.ParamCount != 0)
                            return ParseResult.FromError("Usage: \"manage-other-grids\"");
                        _settings.ManageOtherGrids = true;
                        return ParseResult.Success;

                    case "auto-manage-this-grid":
                        if (configLine.ParamCount != 0)
                            return ParseResult.FromError("Usage: \"auto-manage-this-grid\"");
                        _settings.AutoManageThisGrid = true;
                        return ParseResult.Success;

                    case "auto-manage-other-grids":
                        if (configLine.ParamCount != 0)
                            return ParseResult.FromError("Usage: \"auto-manage-other-grids\"");
                        _settings.AutoManageOtherGrids = true;
                        return ParseResult.Success;

                    default:
                        return ParseResult.Ignored;
                }
            }

            public void OnCompleted() { }

            private ManagerSettings _settings;
        }
    }
}
