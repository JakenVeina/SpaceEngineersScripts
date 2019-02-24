using NUnit.Framework;
using Shouldly;

using static IngameScript.Program;

namespace Mixins.Test
{
    [TestFixture]
    public class BlockCollectionResultTests
    {
        #region Success Tests

        [Test]
        public void Success_Always_IsSuccess()
        {
            var result = BlockCollectionResult.Success;

            result.IsSuccess.ShouldBeTrue();
            result.IsIgnored.ShouldBeFalse();
            result.IsSkipped.ShouldBeFalse();
        }

        #endregion Success Tests

        #region Ignored Tests

        [Test]
        public void Ignored_Always_IsSuccess()
        {
            var result = BlockCollectionResult.Ignored;

            result.IsSuccess.ShouldBeFalse();
            result.IsIgnored.ShouldBeTrue();
            result.IsSkipped.ShouldBeFalse();
        }

        #endregion Ignored Tests

        #region Skipped Tests

        [Test]
        public void Skipped_Always_IsSkipped()
        {
            var result = BlockCollectionResult.Skipped;

            result.IsSuccess.ShouldBeFalse();
            result.IsIgnored.ShouldBeFalse();
            result.IsSkipped.ShouldBeTrue();
        }

        #endregion Skipped Tests
    }
}
