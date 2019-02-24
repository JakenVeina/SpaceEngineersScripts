using Moq;
using NUnit.Framework;
using Shouldly;

using static IngameScript.Program;

namespace Mixins.Test
{
    [TestFixture]
    public class ManagedBlockSettingsProviderTests
    {
        #region Settings Tests

        [TestCase(false, false)]
        [TestCase(true,  false)]
        [TestCase(false, true)]
        [TestCase(true,  true)]
        public void Settings_Always_SavesValue(bool managed, bool ignore)
        {
            var uut = new ManagedBlockSettingsProvider();

            var settings = new ManagedBlockSettings()
            {
                Manage = managed,
                Ignore = ignore
            };

            uut.Settings = settings;

            uut.Settings.ShouldBe(settings);
        }

        #endregion Settings Tests

        #region OnStarting() Tests

        [Test]
        public void OnStarting_Always_ResetsSettings()
        {
            var uut = new ManagedBlockSettingsProvider();

            uut.OnStarting();

            uut.Settings.Ignore.ShouldBeFalse();
            uut.Settings.Manage.ShouldBeFalse();
        }

        #endregion OnStarting() Tests

        #region OnParsing() Tests

        [TestCase("blockTag", "unknown-option")]
        [TestCase("blockTag", "unknown-option", "unknown-param")]
        public void OnParsing_OptionIsUnknown_ReturnsIgnoredAndSetsManage(params string[] linePieces)
        {
            var configLine = new ManagedBlockConfigLine(linePieces);

            var uut = new ManagedBlockSettingsProvider();

            var settings = uut.Settings;
            settings.Manage = true;

            var result = uut.OnParsing(configLine);

            result.IsIgnored.ShouldBeTrue();

            uut.Settings.ShouldBe(settings);
        }

        [TestCase("blockTag", "manage", "bogus-param")]
        [TestCase("blockTag", "ignore", "bogus-param")]
        public void OnParsing_ParamsAreInvalid_ReturnsErrorAndDoesNotChangeSettings(params string[] linePieces)
        {
            var configLine = new ManagedBlockConfigLine(linePieces);

            var uut = new ManagedBlockSettingsProvider();

            var settings = uut.Settings;

            var result = uut.OnParsing(configLine);

            result.IsError.ShouldBeTrue();
            if(linePieces.Length >= 2)
                result.Error.ShouldContain(linePieces[1]);

            uut.Settings.ShouldBe(settings);
        }

        [TestCase("blockTag", "manage")]
        public void OnParsing_ParamsAreValidForManage_ReturnsSuccessAndSetsManage(params string[] linePieces)
        {
            var configLine = new ManagedBlockConfigLine(linePieces);

            var uut = new ManagedBlockSettingsProvider();

            var settings = uut.Settings;
            settings.Manage = true;

            var result = uut.OnParsing(configLine);

            result.IsSuccess.ShouldBeTrue();

            uut.Settings.ShouldBe(settings);
        }

        [TestCase("blockTag", "ignore")]
        public void OnParsing_ParamsAreValidForIgnore_ReturnsSuccessAndSetsIgnore(params string[] linePieces)
        {
            var configLine = new ManagedBlockConfigLine(linePieces);

            var uut = new ManagedBlockSettingsProvider();

            var settings = uut.Settings;
            settings.Ignore = true;

            var result = uut.OnParsing(configLine);

            result.IsSuccess.ShouldBeTrue();

            uut.Settings.ShouldBe(settings);
        }

        #endregion OnParsing() Tests

        #region OnCompleted() Tests

        [TestCase(false, false)]
        [TestCase(true,  false)]
        [TestCase(false, true )]
        [TestCase(true,  true )]
        public void OnCompleted_Always_DoesNothing(bool ignore, bool manage)
        {
            var uut = new ManagedBlockSettingsProvider();

            var settings = new ManagedBlockSettings()
            {
                Ignore = ignore,
                Manage = manage
            };
            uut.Settings = settings;

            uut.OnCompleted();

            uut.Settings.ShouldBe(settings);
        }

        #endregion OnCompleted() Tests
    }

    public static class ManagedBlockSettingsAssertions
    {
        public static void ShouldBe(this ManagedBlockSettings settings, ManagedBlockSettings expected)
        {
            settings.Manage.ShouldBe(expected.Manage);
            settings.Ignore.ShouldBe(expected.Ignore);
        }
    }
}
