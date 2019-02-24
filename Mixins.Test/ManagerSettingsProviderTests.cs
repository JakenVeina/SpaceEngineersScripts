using NUnit.Framework;
using Shouldly;

using static IngameScript.Program;

namespace Mixins.Test
{
    [TestFixture]
    public class ManagerSettingsProviderTests
    {
        #region Settings Tests

        [TestCase("blockTag1", false, false, false)]
        [TestCase("blockTag2", true,  false, false)]
        [TestCase("blockTag3", false, true,  false)]
        [TestCase("blockTag4", false, false, true)]
        [TestCase("blockTag5", true,  true,  true)]
        public void Settings_Always_SavesValue(string blockTag, bool manageOtherGrids, bool autoManageThisGrid, bool autoManageOtherGrids)
        {
            var uut = new ManagerSettingsProvider();

            var settings = new ManagerSettings()
            {
                BlockTag = blockTag,
                ManageOtherGrids = manageOtherGrids,
                AutoManageThisGrid = autoManageThisGrid,
                AutoManageOtherGrids = autoManageOtherGrids
            };

            uut.Settings = settings;

            uut.Settings.ShouldBe(settings);
        }

        #endregion Settings Tests

        #region OnStarting() Tests

        [TestCase("DefaultBlockTag1")]
        [TestCase("DefaultBlockTag2")]
        public void OnStarting_Always_ResetsSettings(string defualtBlockTag)
        {
            var uut = new ManagerSettingsProvider()
            {
                DefaultBlockTag = defualtBlockTag
            };

            uut.OnStarting();

            uut.Settings.BlockTag.ShouldBe(defualtBlockTag);
            uut.Settings.ManageOtherGrids.ShouldBe(false);
            uut.Settings.AutoManageThisGrid.ShouldBe(true);
            uut.Settings.AutoManageOtherGrids.ShouldBe(false);
        }

        #endregion OnStarting() Tests

        #region OnParsing() Tests

        [TestCase("unknown-option")]
        [TestCase("unknown-option", "unknown-param")]
        public void OnParsing_OptionIsUnknown_ReturnsIgnoredAndDoesNotChangeSettings(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new ManagerSettingsProvider();

            var settings = uut.Settings;

            var result = uut.OnParsing(configLine);

            result.IsIgnored.ShouldBeTrue();

            uut.Settings.ShouldBe(settings);
        }

        [TestCase("block-tag")]
        [TestCase("block-tag", "tag", "bogus-param")]
        [TestCase("manage-other-grids", "bogus-param")]
        [TestCase("auto-manage-this-grid", "bogus-param")]
        [TestCase("auto-manage-other-grids", "bogus-param")]
        public void OnParsing_ParamsAreInvalid_ReturnsErrorAndDoesNotChangeSettings(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new ManagerSettingsProvider();

            var settings = uut.Settings;

            var result = uut.OnParsing(configLine);

            result.IsError.ShouldBeTrue();
            result.Error.ShouldContain(linePieces[0]);

            uut.Settings.ShouldBe(settings);
        }

        [TestCase("block-tag", "tag1")]
        [TestCase("block-tag", "tag2")]
        public void OnParsing_ParamsAreValidForBlockTag_ReturnsSuccessAndSetsBlockTag(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new ManagerSettingsProvider();

            var settings = uut.Settings;
            settings.BlockTag = linePieces[1];

            var result = uut.OnParsing(configLine);

            result.IsSuccess.ShouldBeTrue();

            uut.Settings.ShouldBe(settings);
        }

        [TestCase("manage-other-grids")]
        public void OnParsing_ParamsAreValidForManageOtherGrids_ReturnsSuccessAndSetsManageOtherGrids(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new ManagerSettingsProvider();

            var settings = uut.Settings;
            settings.ManageOtherGrids = true;

            var result = uut.OnParsing(configLine);

            result.IsSuccess.ShouldBeTrue();

            uut.Settings.ShouldBe(settings);
        }

        [TestCase("auto-manage-this-grid")]
        public void OnParsing_ParamsAreValidForAutoManageThisGrid_ReturnsSuccessAndSetsAutoManageThisGrid(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new ManagerSettingsProvider();

            var settings = uut.Settings;
            settings.AutoManageThisGrid = true;

            var result = uut.OnParsing(configLine);

            result.IsSuccess.ShouldBeTrue();

            uut.Settings.ShouldBe(settings);
        }

        [TestCase("auto-manage-other-grids")]
        public void OnParsing_ParamsAreValidForAutoManageOtherGrids_ReturnsSuccessAndSetsAutoManageOtherGrids(params string[] linePieces)
        {
            var configLine = new ConfigLine(linePieces);

            var uut = new ManagerSettingsProvider();

            var settings = uut.Settings;
            settings.AutoManageOtherGrids = true;

            var result = uut.OnParsing(configLine);

            result.IsSuccess.ShouldBeTrue();

            uut.Settings.ShouldBe(settings);
        }

        #endregion OnParsing() Tests

        #region OnCompleted() Tests

        [TestCase("blockTag1", false, false, false)]
        [TestCase("blockTag2", true,  false, false)]
        [TestCase("blockTag3", false, true,  false)]
        [TestCase("blockTag4", false, false, true)]
        [TestCase("blockTag5", true,  true,  true)]
        public void OnCompleted_Always_DoesNothing(string blockTag, bool manageOtherGrids, bool autoManageThisGrid, bool autoManageOtherGrids)
        {
            var uut = new ManagerSettingsProvider();

            var settings = new ManagerSettings()
            {
                BlockTag = blockTag,
                ManageOtherGrids = manageOtherGrids,
                AutoManageThisGrid = autoManageThisGrid,
                AutoManageOtherGrids = autoManageOtherGrids
            };
            uut.Settings = settings;

            uut.OnCompleted();

            uut.Settings.ShouldBe(settings);
        }

        #endregion OnCompleted() Tests
    }

    public static class ManagerSettingsAssertions
    {
        public static void ShouldBe(this ManagerSettings settings, ManagerSettings expected)
        {
            settings.BlockTag.ShouldBe(expected.BlockTag);
            settings.ManageOtherGrids.ShouldBe(expected.ManageOtherGrids);
            settings.AutoManageThisGrid.ShouldBe(expected.AutoManageThisGrid);
            settings.AutoManageOtherGrids.ShouldBe(expected.AutoManageOtherGrids);
        }
    }
}
