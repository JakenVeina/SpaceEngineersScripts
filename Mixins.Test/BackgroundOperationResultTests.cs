using NUnit.Framework;
using Shouldly;

using static IngameScript.Program;

namespace Mixins.Test
{
    [TestFixture]
    public class BackgroundOperationResultTests
    {
        #region Completed Tests

        [Test]
        public void Completed_Always_IsComplete()
        {
            var result = BackgroundOperationResult.Completed;

            result.IsComplete.ShouldBeTrue();
            result.IsIncomplete.ShouldBeFalse();
        }

        #endregion Completed Tests

        #region NotCompleted Tests

        [Test]
        public void NotCompleted_Always_IsIncomplete()
        {
            var result = BackgroundOperationResult.NotCompleted;

            result.IsIncomplete.ShouldBeTrue();
            result.IsComplete.ShouldBeFalse();
        }

        #endregion NotCompleted Tests

        #region Constructor Tests

        [TestCase(true)]
        [TestCase(false)]
        public void Constructor_Always_IsCompleteIsGiven(bool isComplete)
        {
            var result = new BackgroundOperationResult(isComplete);

            result.IsComplete.ShouldBe(isComplete);
            result.IsIncomplete.ShouldBe(!isComplete);
        }

        #endregion Constructor Tests
    }
}
