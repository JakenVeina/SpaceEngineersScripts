using NUnit.Framework;
using Shouldly;

using static IngameScript.Program;

namespace AutomatedDockingProcedures.Test
{
    [TestFixture]
    public class DockingManagerSettingsProviderTests
    {
        #region Settings Tests

        [TestCase(false, false, false, false, false, false, false, false, false, false)]
        [TestCase(true,  false, false, false, false, false, false, false, false, false)]
        [TestCase(false, true,  false, false, false, false, false, false, false, false)]
        [TestCase(false, false, true,  false, false, false, false, false, false, false)]
        [TestCase(false, false, false, true,  false, false, false, false, false, false)]
        [TestCase(false, false, false, false, true,  false, false, false, false, false)]
        [TestCase(false, false, false, false, false, true,  false, false, false, false)]
        [TestCase(false, false, false, false, false, false, true,  false, false, false)]
        [TestCase(false, false, false, false, false, false, false, true,  false, false)]
        [TestCase(false, false, false, false, false, false, false, false, true,  false)]
        [TestCase(false, false, false, false, false, false, false, false, false, true )]
        [TestCase(true,  true,  true,  true,  true,  true,  true,  true,  true,  true )]
        public void Settings_Always_SavesValue(
            bool ignoreBatteryBlocks,
            bool ignoreBeacons,
            bool ignoreGasGenerators,
            bool ignoreGasTanks,
            bool ignoreGyros,
            bool ignoreLandingGears,
            bool ignoreLightingBlocks,
            bool ignoreRadioAntennae,
            bool ignoreReactors,
            bool ignoreThrusters)
        {
            var uut = new DockingManagerSettingsProvider();

            var settings = new DockingManagerSettings()
            {
                IgnoreBatteryBlocks  = ignoreBatteryBlocks,
                IgnoreBeacons        = ignoreBeacons,
                IgnoreGasGenerators  = ignoreGasGenerators,
                IgnoreGasTanks       = ignoreGasTanks,
                IgnoreGyros          = ignoreGyros,
                IgnoreLandingGears   = ignoreLandingGears,
                IgnoreLightingBlocks = ignoreLightingBlocks,
                IgnoreRadioAntennae  = ignoreRadioAntennae,
                IgnoreReactors       = ignoreReactors,
                IgnoreThrusters      = ignoreThrusters
            };

            uut.Settings = settings;

            uut.Settings.ShouldBe(settings);
        }

        #endregion Settings Tests

        #region OnStarting() Tests

        [Test]
        public void OnStarting_Always_ResetsSettings()
        {
            var uut = new DockingManagerSettingsProvider();

            uut.OnStarting();

            uut.Settings.IgnoreBatteryBlocks .ShouldBe(false);
            uut.Settings.IgnoreBeacons       .ShouldBe(false);
            uut.Settings.IgnoreGasGenerators .ShouldBe(false);
            uut.Settings.IgnoreGasTanks      .ShouldBe(false);
            uut.Settings.IgnoreGyros         .ShouldBe(false);
            uut.Settings.IgnoreLandingGears  .ShouldBe(false);
            uut.Settings.IgnoreLightingBlocks.ShouldBe(false);
            uut.Settings.IgnoreRadioAntennae .ShouldBe(false);
            uut.Settings.IgnoreReactors      .ShouldBe(false);
            uut.Settings.IgnoreThrusters     .ShouldBe(false);
        }

        #endregion OnStarting() Tests

        #region OnParsing() Tests

        [TestCase("unknown-option")]
        [TestCase("unknown-option", "unknown-param")]
        public void OnParsing_OptionIsUnknown_ReturnsIgnoredAndDoesNotChangeSettings(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new DockingManagerSettingsProvider();

            var settings = uut.Settings;

            var result = uut.OnParsing(configLine);

            result.IsIgnored.ShouldBeTrue();

            uut.Settings.ShouldBe(settings);
        }


        [TestCase("ignore-battery-blocks",  "bogus-param")]
        [TestCase("ignore-beacons",         "bogus-param")]
        [TestCase("ignore-gas-generators",  "bogus-param")]
        [TestCase("ignore-gas-tanks",       "bogus-param")]
        [TestCase("ignore-gyros",           "bogus-param")]
        [TestCase("ignore-landing-gears",   "bogus-param")]
        [TestCase("ignore-lighting-blocks", "bogus-param")]
        [TestCase("ignore-radio-antennae",  "bogus-param")]
        [TestCase("ignore-reactors",        "bogus-param")]
        [TestCase("ignore-thrusters",       "bogus-param")]
        public void OnParsing_ParamsAreInvalid_ReturnsErrorAndDoesNotChangeSettings(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new DockingManagerSettingsProvider();

            var settings = uut.Settings;

            var result = uut.OnParsing(configLine);

            result.IsError.ShouldBeTrue();
            result.Error.ShouldContain(linePieces[0]);

            uut.Settings.ShouldBe(settings);
        }

        [TestCase("ignore-battery-blocks")]
        public void OnParsing_ParamsAreValidForIgnoreBatteryBlocks_ReturnsSuccessAndSetsIgnoreBatteryBlocks(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new DockingManagerSettingsProvider();

            var settings = uut.Settings;
            settings.IgnoreBatteryBlocks = true;

            var result = uut.OnParsing(configLine);

            result.IsSuccess.ShouldBeTrue();

            uut.Settings.ShouldBe(settings);
        }

        [TestCase("ignore-beacons")]
        public void OnParsing_ParamsAreValidForIgnoreBeacons_ReturnsSuccessAndSetsIgnoreBeacons(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new DockingManagerSettingsProvider();

            var settings = uut.Settings;
            settings.IgnoreBeacons = true;

            var result = uut.OnParsing(configLine);

            result.IsSuccess.ShouldBeTrue();

            uut.Settings.ShouldBe(settings);
        }

        [TestCase("ignore-gas-generators")]
        public void OnParsing_ParamsAreValidForIgnoreGasGenerators_ReturnsSuccessAndSetsIgnoreGasGenerators(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new DockingManagerSettingsProvider();

            var settings = uut.Settings;
            settings.IgnoreGasGenerators = true;

            var result = uut.OnParsing(configLine);

            result.IsSuccess.ShouldBeTrue();

            uut.Settings.ShouldBe(settings);
        }

        [TestCase("ignore-gas-tanks")]
        public void OnParsing_ParamsAreValidForIgnoreGasTanks_ReturnsSuccessAndSetsIgnoreGasTanks(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new DockingManagerSettingsProvider();

            var settings = uut.Settings;
            settings.IgnoreGasTanks = true;

            var result = uut.OnParsing(configLine);

            result.IsSuccess.ShouldBeTrue();

            uut.Settings.ShouldBe(settings);
        }

        [TestCase("ignore-gyros")]
        public void OnParsing_ParamsAreValidForIgnoreGyros_ReturnsSuccessAndSetsIgnoreGyros(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new DockingManagerSettingsProvider();

            var settings = uut.Settings;
            settings.IgnoreGyros = true;

            var result = uut.OnParsing(configLine);

            result.IsSuccess.ShouldBeTrue();

            uut.Settings.ShouldBe(settings);
        }

        [TestCase("ignore-landing-gears")]
        public void OnParsing_ParamsAreValidForIgnoreLandingGears_ReturnsSuccessAndSetsIgnoreLandingGears(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new DockingManagerSettingsProvider();

            var settings = uut.Settings;
            settings.IgnoreLandingGears = true;

            var result = uut.OnParsing(configLine);

            result.IsSuccess.ShouldBeTrue();

            uut.Settings.ShouldBe(settings);
        }

        [TestCase("ignore-lighting-blocks")]
        public void OnParsing_ParamsAreValidForIgnoreLightingBlocks_ReturnsSuccessAndSetsIgnoreLightingBlocks(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new DockingManagerSettingsProvider();

            var settings = uut.Settings;
            settings.IgnoreLightingBlocks = true;

            var result = uut.OnParsing(configLine);

            result.IsSuccess.ShouldBeTrue();

            uut.Settings.ShouldBe(settings);
        }

        [TestCase("ignore-radio-antennae")]
        public void OnParsing_ParamsAreValidForIgnoreRadioAntennae_ReturnsSuccessAndSetsIgnoreRadioAntennae(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new DockingManagerSettingsProvider();

            var settings = uut.Settings;
            settings.IgnoreRadioAntennae = true;

            var result = uut.OnParsing(configLine);

            result.IsSuccess.ShouldBeTrue();

            uut.Settings.ShouldBe(settings);
        }

        [TestCase("ignore-reactors")]
        public void OnParsing_ParamsAreValidForIgnoreReactors_ReturnsSuccessAndSetsIgnoreReactors(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new DockingManagerSettingsProvider();

            var settings = uut.Settings;
            settings.IgnoreReactors = true;

            var result = uut.OnParsing(configLine);

            result.IsSuccess.ShouldBeTrue();

            uut.Settings.ShouldBe(settings);
        }

        [TestCase("ignore-thrusters")]
        public void OnParsing_ParamsAreValidForIgnoreThrusters_ReturnsSuccessAndSetsIgnoreThrusters(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new DockingManagerSettingsProvider();

            var settings = uut.Settings;
            settings.IgnoreThrusters = true;

            var result = uut.OnParsing(configLine);

            result.IsSuccess.ShouldBeTrue();

            uut.Settings.ShouldBe(settings);
        }

        #endregion OnParsing() Tests

        #region OnCompleted() Tests

        [TestCase(false, false, false, false, false, false, false, false, false, false)]
        [TestCase(true,  false, false, false, false, false, false, false, false, false)]
        [TestCase(false, true,  false, false, false, false, false, false, false, false)]
        [TestCase(false, false, true,  false, false, false, false, false, false, false)]
        [TestCase(false, false, false, true,  false, false, false, false, false, false)]
        [TestCase(false, false, false, false, true,  false, false, false, false, false)]
        [TestCase(false, false, false, false, false, true,  false, false, false, false)]
        [TestCase(false, false, false, false, false, false, true,  false, false, false)]
        [TestCase(false, false, false, false, false, false, false, true,  false, false)]
        [TestCase(false, false, false, false, false, false, false, false, true,  false)]
        [TestCase(false, false, false, false, false, false, false, false, false, true )]
        [TestCase(true,  true,  true,  true,  true,  true,  true,  true,  true,  true )]
        public void OnCompleted_Always_DoesNothing(
            bool ignoreBatteryBlocks,
            bool ignoreBeacons,
            bool ignoreGasGenerators,
            bool ignoreGasTanks,
            bool ignoreGyros,
            bool ignoreLandingGears,
            bool ignoreLightingBlocks,
            bool ignoreRadioAntennae,
            bool ignoreReactors,
            bool ignoreThrusters)
        {
            var uut = new DockingManagerSettingsProvider();

            var settings = new DockingManagerSettings()
            {
                IgnoreBatteryBlocks  = ignoreBatteryBlocks,
                IgnoreBeacons        = ignoreBeacons,
                IgnoreGasGenerators  = ignoreGasGenerators,
                IgnoreGasTanks       = ignoreGasTanks,
                IgnoreGyros          = ignoreGyros,
                IgnoreLandingGears   = ignoreLandingGears,
                IgnoreLightingBlocks = ignoreLightingBlocks,
                IgnoreRadioAntennae  = ignoreRadioAntennae,
                IgnoreReactors       = ignoreReactors,
                IgnoreThrusters      = ignoreThrusters
            };

            uut.OnCompleted();

            uut.Settings.ShouldBe(settings);
        }

        #endregion OnCompleted() Tests
    }

    public static class DockingManagerSettingsAssertions
    {
        public static void ShouldBe(this DockingManagerSettings settings, DockingManagerSettings expected)
        {
            settings.IgnoreBatteryBlocks .ShouldBe(settings.IgnoreBatteryBlocks);
            settings.IgnoreBeacons       .ShouldBe(settings.IgnoreBeacons);
            settings.IgnoreGasGenerators .ShouldBe(settings.IgnoreGasGenerators);
            settings.IgnoreGasTanks      .ShouldBe(settings.IgnoreGasTanks);
            settings.IgnoreGyros         .ShouldBe(settings.IgnoreGyros);
            settings.IgnoreLandingGears  .ShouldBe(settings.IgnoreLandingGears);
            settings.IgnoreLightingBlocks.ShouldBe(settings.IgnoreLightingBlocks);
            settings.IgnoreRadioAntennae .ShouldBe(settings.IgnoreRadioAntennae);
            settings.IgnoreReactors      .ShouldBe(settings.IgnoreReactors);
            settings.IgnoreThrusters     .ShouldBe(settings.IgnoreThrusters);
        }
    }
}
