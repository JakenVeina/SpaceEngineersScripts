using System;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public interface IProgramSettingsProvider
        {
            ProgramSettings Settings { get; set;  }
        }

        public class ProgramSettingsProvider : IProgramSettingsProvider, IConfigParseHandler
        {
            public ProgramSettings Settings
            {
                get { return _settings; }
                set { _settings = value; }
            }

            public void OnStarting()
            {
                _settings = new ProgramSettings()
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
                Func<ProgramSettingsProvider, ConfigLine, ParseResult> parser;
                if (_parsersByOption.TryGetValue(configLine.Option, out parser))
                    return parser.Invoke(this, configLine);

                return ParseResult.Ignored;
            }

            public void OnCompleted() { }

            private static ParseResult ParseIgnoreBatteryBlocks(ProgramSettingsProvider @this, ConfigLine configLine)
            {
                if (configLine.ParamCount != 0)
                    return ParseResult.FromError("Usage: \"ignore-battery-blocks\"");
                @this._settings.IgnoreBatteryBlocks = true;
                return ParseResult.Success;
            }

            private static ParseResult ParseIgnoreBeacons(ProgramSettingsProvider @this, ConfigLine configLine)
            {
                if (configLine.ParamCount != 0)
                    return ParseResult.FromError("Usage: \"ignore-beacons\"");
                @this._settings.IgnoreBeacons = true;
                return ParseResult.Success;
            }

            private static ParseResult ParseIgnoreGasGenerators(ProgramSettingsProvider @this, ConfigLine configLine)
            {
                if (configLine.ParamCount != 0)
                    return ParseResult.FromError("Usage: \"ignore-gas-generators\"");
                @this._settings.IgnoreGasGenerators = true;
                return ParseResult.Success;
            }

            private static ParseResult ParseIgnoreGasTanks(ProgramSettingsProvider @this, ConfigLine configLine)
            {
                if (configLine.ParamCount != 0)
                    return ParseResult.FromError("Usage: \"ignore-gas-tanks\"");
                @this._settings.IgnoreGasTanks = true;
                return ParseResult.Success;
            }

            private static ParseResult ParseIgnoreGyros(ProgramSettingsProvider @this, ConfigLine configLine)
            {
                if (configLine.ParamCount != 0)
                    return ParseResult.FromError("Usage: \"ignore-gyros\"");
                @this._settings.IgnoreGyros = true;
                return ParseResult.Success;
            }

            private static ParseResult ParseIgnoreLandingGears(ProgramSettingsProvider @this, ConfigLine configLine)
            {
                if (configLine.ParamCount != 0)
                    return ParseResult.FromError("Usage: \"ignore-landing-gears\"");
                @this._settings.IgnoreLandingGears = true;
                return ParseResult.Success;
            }

            private static ParseResult ParseIgnoreLightingBlocks(ProgramSettingsProvider @this, ConfigLine configLine)
            {
                if (configLine.ParamCount != 0)
                    return ParseResult.FromError("Usage: \"ignore-lighting-blocks\"");
                @this._settings.IgnoreLightingBlocks = true;
                return ParseResult.Success;
            }

            private static ParseResult ParseIgnoreRadioAntennae(ProgramSettingsProvider @this, ConfigLine configLine)
            {
                if (configLine.ParamCount != 0)
                    return ParseResult.FromError("Usage: \"ignore-radio-antennae\"");
                @this._settings.IgnoreRadioAntennae = true;
                return ParseResult.Success;
            }

            private static ParseResult ParseIgnoreReactors(ProgramSettingsProvider @this, ConfigLine configLine)
            {
                if (configLine.ParamCount != 0)
                    return ParseResult.FromError("Usage: \"ignore-reactors\"");
                @this._settings.IgnoreReactors = true;
                return ParseResult.Success;
            }

            private ProgramSettings _settings;

            private static readonly Dictionary<string, Func<ProgramSettingsProvider, ConfigLine, ParseResult>> _parsersByOption
                = new Dictionary<string, Func<ProgramSettingsProvider, ConfigLine, ParseResult>>()
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
