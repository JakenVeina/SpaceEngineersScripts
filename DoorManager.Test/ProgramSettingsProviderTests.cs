using System;

using NUnit.Framework;
using Shouldly;

using static IngameScript.Program;

namespace DoorManager.Test
{
    [TestFixture]
    public class ProgramSettingsProviderTests
    {
        #region Settings Tests

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        public void Settings_Always_SavesValue(int manageIntervalMilliseconds)
        {
            var uut = new ProgramSettingsProvider();

            var settings = new ProgramSettings()
            {
                ManageInterval = TimeSpan.FromMilliseconds(manageIntervalMilliseconds)
            };

            uut.Settings = settings;

            uut.Settings.ShouldBe(settings);
        }

        #endregion Settings Tests

        #region OnStarting() Tests

        [Test]
        public void OnStarting_Always_ResetsSettings()
        {
            var uut = new ProgramSettingsProvider();

            uut.OnStarting();

            uut.Settings.ManageInterval.ShouldNotBe(TimeSpan.Zero);
        }

        #endregion OnStarting() Tests

        #region OnParsing() Tests

        [TestCase("unknown-option")]
        [TestCase("unknown-option", "unknown-param")]
        public void OnParsing_OptionIsUnknown_ReturnsIgnoredAndDoesNotChangeSettings(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new ProgramSettingsProvider();

            var settings = uut.Settings;

            var result = uut.OnParsing(configLine);

            result.IsIgnored.ShouldBeTrue();

            uut.Settings.ShouldBe(settings);
        }

        [TestCase("manage-interval")]
        [TestCase("manage-interval", "1", "bogus-param")]
        [TestCase("manage-interval", "bogus-value")]
        [TestCase("manage-interval", "-1")]
        [TestCase("manage-interval", "0")]
        public void OnParsing_ParamsAreInvalid_ReturnsErrorAndDoesNotChangeSettings(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new ProgramSettingsProvider();

            var settings = uut.Settings;

            var result = uut.OnParsing(configLine);

            result.IsError.ShouldBeTrue();
            result.Error.ShouldContain(configLine.Option);

            uut.Settings.ShouldBe(settings);
        }

        [TestCase("manage-interval", "1")]
        [TestCase("manage-interval", "10")]
        [TestCase("manage-interval", "100")]
        public void OnParsing_ParamsAreValidForBlockTag_ReturnsSuccessAndSetsBlockTag(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new ProgramSettingsProvider();

            var settings = uut.Settings;
            settings.ManageInterval = TimeSpan.FromMilliseconds(int.Parse(configLine.GetParam(0)));

            var result = uut.OnParsing(configLine);

            result.IsSuccess.ShouldBeTrue();

            uut.Settings.ShouldBe(settings);
        }

        #endregion OnParsing() Tests

        #region OnCompleted() Tests

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        public void OnCompleted_Always_DoesNothing(int manageIntervalMilliseconds)
        {
            var uut = new ProgramSettingsProvider();

            var settings = new ProgramSettings()
            {
                ManageInterval = TimeSpan.FromMilliseconds(manageIntervalMilliseconds)
            };
            uut.Settings = settings;

            uut.OnCompleted();

            uut.Settings.ShouldBe(settings);
        }

        #endregion OnCompleted() Tests
    }

    public static class ProgramSettingsAssertions
    {
        public static void ShouldBe(this ProgramSettings settings, ProgramSettings expected)
        {
            settings.ManageInterval.ShouldBe(expected.ManageInterval);
        }
    }
}
