using System;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public interface IDockingManagerSettingsProvider
        {
            DockingManagerSettings Settings { get; set;  }
        }

        public class DockingManagerSettingsProvider : IDockingManagerSettingsProvider, IConfigParseHandler
        {
            public DockingManagerSettings Settings
            {
                get { return _settings; }
                set { _settings = value; }
            }

            public void OnStarting()
            {
                _settings = new DockingManagerSettings()
                {
                    IgnoreBatteryBlocks  = false,
                    IgnoreBeacons        = false,
                    IgnoreGasGenerators  = false,
                    IgnoreGasTanks       = false,
                    IgnoreGyros          = false,
                    IgnoreLandingGears   = false,
                    IgnoreLightingBlocks = false,
                    IgnoreRadioAntennae  = false,
                    IgnoreReactors       = false
                };
            }

            public ParseResult OnParsing(ConfigLine configLine)
            {
                Func<DockingManagerSettingsProvider, ConfigLine, ParseResult> parser;
                if (_parsersByOption.TryGetValue(configLine.Option, out parser))
                    return parser.Invoke(this, configLine);

                return ParseResult.Ignored;
            }

            public void OnCompleted() { }

            private static ParseResult ParseIgnoreBatteryBlocks(DockingManagerSettingsProvider @this, ConfigLine configLine)
            {
                if (configLine.ParamCount != 0)
                    return ParseResult.FromError("Usage: \"ignore-battery-blocks\"");
                @this._settings.IgnoreBatteryBlocks = true;
                return ParseResult.Success;
            }

            private static ParseResult ParseIgnoreBeacons(DockingManagerSettingsProvider @this, ConfigLine configLine)
            {
                if (configLine.ParamCount != 0)
                    return ParseResult.FromError("Usage: \"ignore-beacons\"");
                @this._settings.IgnoreBeacons = true;
                return ParseResult.Success;
            }

            private static ParseResult ParseIgnoreGasGenerators(DockingManagerSettingsProvider @this, ConfigLine configLine)
            {
                if (configLine.ParamCount != 0)
                    return ParseResult.FromError("Usage: \"ignore-gas-generators\"");
                @this._settings.IgnoreGasGenerators = true;
                return ParseResult.Success;
            }

            private static ParseResult ParseIgnoreGasTanks(DockingManagerSettingsProvider @this, ConfigLine configLine)
            {
                if (configLine.ParamCount != 0)
                    return ParseResult.FromError("Usage: \"ignore-gas-tanks\"");
                @this._settings.IgnoreGasTanks = true;
                return ParseResult.Success;
            }

            private static ParseResult ParseIgnoreGyros(DockingManagerSettingsProvider @this, ConfigLine configLine)
            {
                if (configLine.ParamCount != 0)
                    return ParseResult.FromError("Usage: \"ignore-gyros\"");
                @this._settings.IgnoreGyros = true;
                return ParseResult.Success;
            }

            private static ParseResult ParseIgnoreLandingGears(DockingManagerSettingsProvider @this, ConfigLine configLine)
            {
                if (configLine.ParamCount != 0)
                    return ParseResult.FromError("Usage: \"ignore-landing-gears\"");
                @this._settings.IgnoreLandingGears = true;
                return ParseResult.Success;
            }

            private static ParseResult ParseIgnoreLightingBlocks(DockingManagerSettingsProvider @this, ConfigLine configLine)
            {
                if (configLine.ParamCount != 0)
                    return ParseResult.FromError("Usage: \"ignore-lighting-blocks\"");
                @this._settings.IgnoreLightingBlocks = true;
                return ParseResult.Success;
            }

            private static ParseResult ParseIgnoreRadioAntennae(DockingManagerSettingsProvider @this, ConfigLine configLine)
            {
                if (configLine.ParamCount != 0)
                    return ParseResult.FromError("Usage: \"ignore-radio-antennae\"");
                @this._settings.IgnoreRadioAntennae = true;
                return ParseResult.Success;
            }

            private static ParseResult ParseIgnoreReactors(DockingManagerSettingsProvider @this, ConfigLine configLine)
            {
                if (configLine.ParamCount != 0)
                    return ParseResult.FromError("Usage: \"ignore-reactors\"");
                @this._settings.IgnoreReactors = true;
                return ParseResult.Success;
            }

            private DockingManagerSettings _settings;

            private static readonly Dictionary<string, Func<DockingManagerSettingsProvider, ConfigLine, ParseResult>> _parsersByOption
                = new Dictionary<string, Func<DockingManagerSettingsProvider, ConfigLine, ParseResult>>()
                {
                    ["ignore-battery-blocks"]  = ParseIgnoreBatteryBlocks,
                    ["ignore-beacons"]         = ParseIgnoreBeacons,
                    ["ignore-gas-generators"]  = ParseIgnoreGasGenerators,
                    ["ignore-gas-tanks"]       = ParseIgnoreGasTanks,
                    ["ignore-gyros"]           = ParseIgnoreGyros,
                    ["ignore-landing-gears"]   = ParseIgnoreLandingGears,
                    ["ignore-lighting-blocks"] = ParseIgnoreLightingBlocks,
                    ["ignore-radio-antennae"]  = ParseIgnoreRadioAntennae,
                    ["ignore-reactors"]        = ParseIgnoreReactors
                };
        }
    }
}
