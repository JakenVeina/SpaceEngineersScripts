using System;
using Moq;
using NUnit.Framework;
using Shouldly;

using static IngameScript.Program;

namespace DoorManager.Test
{
    [TestFixture]
    public class ManagedDoorSettingsProviderTests
    {
        #region Test Context

        private class TestContext
        {
            public DoorManagerSettings DoorManagerSettings;

            public TestContext()
            {
                MockDoorManagerSettingsProvider = new Mock<IDoorManagerSettingsProvider>();
                MockDoorManagerSettingsProvider
                    .Setup(x => x.Settings)
                    .Returns(() => DoorManagerSettings);

                Uut = new ManagedDoorSettingsProvider(
                    MockDoorManagerSettingsProvider.Object);
            }

            public readonly Mock<IDoorManagerSettingsProvider> MockDoorManagerSettingsProvider;

            public readonly ManagedDoorSettingsProvider Uut;
        }

        #endregion Test Context

        #region Settings Tests

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        public void Settings_Always_SavesValue(int autoCloseIntervalMilliseconds)
        {
            var testContext = new TestContext();

            var settings = new ManagedDoorSettings()
            {
                AutoCloseInterval = TimeSpan.FromMilliseconds(autoCloseIntervalMilliseconds)
            };

            testContext.Uut.Settings = settings;

            testContext.Uut.Settings.ShouldBe(settings);
        }

        #endregion Settings Tests

        #region OnStarting() Tests

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        public void OnStarting_Always_ResetsSettings(int autoCloseIntervalMilliseconds)
        {
            var testContext = new TestContext()
            {
                DoorManagerSettings = new DoorManagerSettings()
                {
                    AutoCloseInterval = TimeSpan.FromMilliseconds(autoCloseIntervalMilliseconds)
                }
            };

            testContext.Uut.OnStarting();

            testContext.Uut.Settings.AutoCloseInterval.ShouldBe(testContext.DoorManagerSettings.AutoCloseInterval);
        }

        #endregion OnStarting() Tests

        #region OnParsing() Tests

        [TestCase("blockTag", "unknown-option")]
        [TestCase("blockTag", "unknown-option", "unknown-param")]
        public void OnParsing_OptionIsUnknown_ReturnsIgnoredAndDoesNotChangeSettings(params string[] linePieces)
        {
            var configLine = new ManagedBlockConfigLine(linePieces);

            var testContext = new TestContext();

            var settings = testContext.Uut.Settings;

            var result = testContext.Uut.OnParsing(configLine);

            result.IsIgnored.ShouldBeTrue();

            testContext.Uut.Settings.ShouldBe(settings);
        }

        [TestCase("blockTag", "auto-close-interval")]
        [TestCase("blockTag", "auto-close-interval", "1", "bogus-param")]
        [TestCase("blockTag", "auto-close-interval", "bogus-value")]
        [TestCase("blockTag", "auto-close-interval", "-1")]
        [TestCase("blockTag", "auto-close-interval", "0")]
        public void OnParsing_ParamsAreInvalid_ReturnsErrorAndDoesNotChangeSettings(params string[] linePieces)
        {
            var configLine = new ManagedBlockConfigLine(linePieces);

            var testContext = new TestContext();

            var settings = testContext.Uut.Settings;

            var result = testContext.Uut.OnParsing(configLine);

            result.IsError.ShouldBeTrue();
            if(linePieces.Length >= 2)
                result.Error.ShouldContain(linePieces[1]);

            testContext.Uut.Settings.ShouldBe(settings);
        }

        [TestCase("blockTag", "auto-close-interval", "1")]
        [TestCase("blockTag", "auto-close-interval", "10")]
        [TestCase("blockTag", "auto-close-interval", "100")]
        public void OnParsing_ParamsAreValidForAutoCloseInterval_ReturnsSuccessAndSetsAutoCloseInterval(params string[] linePieces)
        {
            var configLine = new ManagedBlockConfigLine(linePieces);

            var testContext = new TestContext();

            var settings = testContext.Uut.Settings;
            settings.AutoCloseInterval = TimeSpan.FromMilliseconds(int.Parse(linePieces[2]));

            var result = testContext.Uut.OnParsing(configLine);

            result.IsSuccess.ShouldBeTrue();

            testContext.Uut.Settings.ShouldBe(settings);
        }

        #endregion OnParsing() Tests

        #region OnCompleted() Tests

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        public void OnCompleted_Always_DoesNothing(int autoCloseIntervalMilliseconds)
        {
            var testContext = new TestContext();

            var settings = new ManagedDoorSettings()
            {
                AutoCloseInterval = TimeSpan.FromMilliseconds(autoCloseIntervalMilliseconds)
            };
            testContext.Uut.Settings = settings;

            testContext.Uut.OnCompleted();

            testContext.Uut.Settings.ShouldBe(settings);
        }

        #endregion OnCompleted() Tests
    }

    public static class ManagedDoorSettingsAssertions
    {
        public static void ShouldBe(this ManagedDoorSettings settings, ManagedDoorSettings expected)
        {
            settings.AutoCloseInterval.ShouldBe(expected.AutoCloseInterval);
        }
    }
}
