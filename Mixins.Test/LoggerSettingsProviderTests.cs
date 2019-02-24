using NUnit.Framework;
using Shouldly;

using static IngameScript.Program;

namespace Mixins.Test
{
    [TestFixture]
    public class LoggerSettingsProviderTests
    {
        #region Settings Tests

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        public void Settings_Always_SavesValue(int maxLogSize)
        {
            var uut = new LoggerSettingsProvider();

            var settings = new LoggerSettings()
            {
                MaxLogSize = maxLogSize
            };

            uut.Settings = settings;

            uut.Settings.ShouldBe(settings);
        }

        #endregion Settings Tests

        #region OnStarting() Tests

        [Test]
        public void OnStarting_Always_ResetsSettings()
        {
            var uut = new LoggerSettingsProvider();

            uut.OnStarting();

            uut.Settings.MaxLogSize.ShouldNotBe(0);
        }

        #endregion OnStarting() Tests

        #region OnParsing() Tests

        [TestCase("unknown-option")]
        [TestCase("unknown-option", "unknown-param")]
        public void OnParsing_OptionIsUnknown_ReturnsIgnoredAndDoesNotChangeMaxLineCount(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new LoggerSettingsProvider();

            var settings = uut.Settings;

            var result = uut.OnParsing(configLine);

            result.IsIgnored.ShouldBeTrue();

            uut.Settings.ShouldBe(settings);
        }

        [TestCase("max-log-size")]
        [TestCase("max-log-size", "invalid-param")]
        [TestCase("max-log-size", "0", "bogus-param")]
        [TestCase("max-log-size", "-1")]
        public void OnParsing_ParamsAreInvalid_ReturnsErrorAndDoesNotChangeSettings(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new LoggerSettingsProvider();

            var settings = uut.Settings;

            var result = uut.OnParsing(configLine);

            result.IsError.ShouldBeTrue();
            result.Error.ShouldContain(linePieces[0]);

            uut.Settings.ShouldBe(settings);
        }

        [TestCase("max-log-size", "0")]
        [TestCase("max-log-size", "1")]
        [TestCase("max-log-size", "5")]
        [TestCase("max-log-size", "10")]
        public void OnParsing_ParamsAreValidForMaxLogSize_ReturnsSuccessAndSetsMaxLogSize(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new LoggerSettingsProvider();

            var settings = uut.Settings;
            settings.MaxLogSize = int.Parse(linePieces[1]);

            var result = uut.OnParsing(configLine);

            result.IsSuccess.ShouldBeTrue();

            uut.Settings.ShouldBe(settings);
        }

        #endregion OnParsing() Tests

        #region OnCompleted() Tests

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        public void OnCompleted_Always_DoesNothing(int maxLogSize)
        {
            var uut = new LoggerSettingsProvider();

            var settings = new LoggerSettings()
            {
                MaxLogSize = maxLogSize
            };
            uut.Settings = settings;

            uut.OnCompleted();

            uut.Settings.ShouldBe(settings);
        }

        #endregion OnCompleted() Tests
    }

    public static class LoggerSettingsAssertions
    {
        public static void ShouldBe(this LoggerSettings settings, LoggerSettings expected)
        {
            settings.MaxLogSize.ShouldBe(expected.MaxLogSize);
        }
    }
}
