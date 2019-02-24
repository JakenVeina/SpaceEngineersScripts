using NUnit.Framework;
using Shouldly;

using static IngameScript.Program;

namespace Mixins.Test
{
    [TestFixture]
    class BackgroundWorkerSettingsProviderTests
    {
        #region Settings Tests

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        public void Settings_Always_SavesValue(int instructionsPerExecution)
        {
            var uut = new BackgroundWorkerSettingsProvider();

            var settings = new BackgroundWorkerSettings()
            {
                InstructionsPerExecution = instructionsPerExecution
            };

            uut.Settings = settings;

            uut.Settings.ShouldBe(settings);
        }

        #endregion Settings Tests

        #region OnStarting() Tests

        [Test]
        public void OnStarting_Always_ResetsSettings()
        {
            var uut = new BackgroundWorkerSettingsProvider();

            uut.OnStarting();

            uut.Settings.InstructionsPerExecution.ShouldNotBe(0);
        }

        #endregion OnStarting() Tests

        #region OnParsing() Tests

        [TestCase("unknown-option")]
        [TestCase("unknown-option", "unknown-param")]
        public void OnParsing_OptionIsUnknown_ReturnsIgnoredAndDoesNotChangeSettings(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new BackgroundWorkerSettingsProvider();

            var settings = uut.Settings;

            var result = uut.OnParsing(configLine);

            result.IsIgnored.ShouldBeTrue();

            uut.Settings.ShouldBe(settings);
        }

        [TestCase("instructions-per-execution")]
        [TestCase("instructions-per-execution", "bogus-value")]
        [TestCase("instructions-per-execution", "1", "bogus-param")]
        [TestCase("instructions-per-execution", "0")]
        [TestCase("instructions-per-execution", "-1")]
        public void OnParsing_ParamsAreInvalid_ReturnsErrorAndDoesNotChangeSettings(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new BackgroundWorkerSettingsProvider();

            var settings = uut.Settings;

            var result = uut.OnParsing(configLine);

            result.IsError.ShouldBeTrue();
            result.Error.ShouldContain(linePieces[0]);

            uut.Settings.ShouldBe(settings);
        }

        [TestCase("instructions-per-execution", "1")]
        [TestCase("instructions-per-execution", "10")]
        [TestCase("instructions-per-execution", "100")]
        public void OnParsing_ParamsAreValidForBlockTag_ReturnsSuccessAndSetsBlockTag(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new BackgroundWorkerSettingsProvider();

            var settings = uut.Settings;
            settings.InstructionsPerExecution = int.Parse(linePieces[1]);

            var result = uut.OnParsing(configLine);

            result.IsSuccess.ShouldBeTrue();

            uut.Settings.ShouldBe(settings);
        }

        #endregion OnParsing() Tests

        #region OnCompleted() Tests

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        public void OnCompleted_Always_DoesNothing(int instructionsPerExecution)
        {
            var uut = new BackgroundWorkerSettingsProvider();

            var settings = new BackgroundWorkerSettings()
            {
                InstructionsPerExecution = instructionsPerExecution
            };
            uut.Settings = settings;

            uut.OnCompleted();

            uut.Settings.ShouldBe(settings);
        }

        #endregion OnCompleted() Tests
    }

    public static class BackgroundWorkerSettingsAssertions
    {
        public static void ShouldBe(this BackgroundWorkerSettings settings, BackgroundWorkerSettings expected)
        {
            settings.InstructionsPerExecution.ShouldBe(expected.InstructionsPerExecution);
        }
    }
}
