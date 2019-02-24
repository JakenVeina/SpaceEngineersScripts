using System;

using NUnit.Framework;
using Shouldly;

using static IngameScript.Program;

namespace DoorManager.Test
{
    [TestFixture]
    public class DoorManagerSettingsProviderTests
    {
        #region Settings Tests

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        public void Settings_Always_SavesValue(int autoCloseIntervalMilliseconds)
        {
            var uut = new DoorManagerSettingsProvider();

            var settings = new DoorManagerSettings()
            {
                AutoCloseInterval = TimeSpan.FromMilliseconds(autoCloseIntervalMilliseconds)
            };

            uut.Settings = settings;

            uut.Settings.ShouldBe(settings);
        }

        #endregion Settings Tests

        #region OnStarting() Tests

        [Test]
        public void OnStarting_Always_ResetsSettings()
        {
            var uut = new DoorManagerSettingsProvider();

            uut.OnStarting();

            uut.Settings.AutoCloseInterval.ShouldNotBe(TimeSpan.Zero);
        }

        #endregion OnStarting() Tests

        #region OnParsing() Tests

        [TestCase("unknown-option")]
        [TestCase("unknown-option", "unknown-param")]
        public void OnParsing_OptionIsUnknown_ReturnsIgnoredAndDoesNotChangeSettings(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new DoorManagerSettingsProvider();

            var settings = uut.Settings;

            var result = uut.OnParsing(configLine);

            result.IsIgnored.ShouldBeTrue();

            uut.Settings.ShouldBe(settings);
        }

        [TestCase("auto-close-interval")]
        [TestCase("auto-close-interval", "1", "bogus-param")]
        [TestCase("auto-close-interval", "bogus-value")]
        [TestCase("auto-close-interval", "-1")]
        [TestCase("auto-close-interval", "0")]
        public void OnParsing_ParamsAreInvalid_ReturnsErrorAndDoesNotChangeSettings(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new DoorManagerSettingsProvider();

            var settings = uut.Settings;

            var result = uut.OnParsing(configLine);

            result.IsError.ShouldBeTrue();
            result.Error.ShouldContain(linePieces[0]);

            uut.Settings.ShouldBe(settings);
        }

        [TestCase("auto-close-interval", "1")]
        [TestCase("auto-close-interval", "10")]
        [TestCase("auto-close-interval", "100")]
        public void OnParsing_ParamsAreValidForAutoCloseInterval_ReturnsSuccessAndSetsAutoCloseInterval(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new DoorManagerSettingsProvider();

            var settings = uut.Settings;
            settings.AutoCloseInterval = TimeSpan.FromMilliseconds(int.Parse(linePieces[1]));

            var result = uut.OnParsing(configLine);

            result.IsSuccess.ShouldBeTrue();

            uut.Settings.ShouldBe(settings);
        }

        #endregion OnParsing() Tests

        #region OnCompleted() Tests

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        public void OnCompleted_Always_DoesNothing(int autoCloseIntervalMilliseconds)
        {
            var uut = new DoorManagerSettingsProvider();

            var settings = new DoorManagerSettings()
            {
                AutoCloseInterval = TimeSpan.FromMilliseconds(autoCloseIntervalMilliseconds)
            };
            uut.Settings = settings;

            uut.OnCompleted();

            uut.Settings.ShouldBe(settings);
        }

        #endregion OnCompleted() Tests
    }

    public static class DoorManagerSettingsAssertions
    {
        public static void ShouldBe(this DoorManagerSettings settings, DoorManagerSettings expected)
        {
            settings.AutoCloseInterval.ShouldBe(expected.AutoCloseInterval);
        }
    }
}
