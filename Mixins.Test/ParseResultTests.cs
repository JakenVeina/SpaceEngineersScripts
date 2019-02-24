using System;

using NUnit.Framework;
using Shouldly;

using static IngameScript.Program;

namespace Mixins.Test
{
    [TestFixture]
    public class ParseResultTests
    {
        #region Success Tests

        [Test]
        public void Success_Always_IsSuccess()
        {
            var uut = ParseResult.Success;

            uut.IsSuccess.ShouldBeTrue();
            uut.IsIgnored.ShouldBeFalse();
            uut.IsError.ShouldBeFalse();
            uut.Error.ShouldBeNull();
        }

        #endregion Success Tests

        #region Ignored Tests

        [Test]
        public void Ignored_Always_IsIgnored()
        {
            var uut = ParseResult.Ignored;

            uut.IsIgnored.ShouldBeTrue();
            uut.IsSuccess.ShouldBeFalse();
            uut.IsError.ShouldBeFalse();
            uut.Error.ShouldBeNull();
        }

        #endregion Ignored Tests

        #region FromError Tests

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        [TestCase("\r\n")]
        [TestCase("This is an error")]
        public void FromError_ErrorIsNotNull_IsError(string error)
        {
            var uut = ParseResult.FromError(error);

            uut.IsError.ShouldBeTrue();
            uut.Error.ShouldBe(error);
            uut.IsSuccess.ShouldBeFalse();
            uut.IsIgnored.ShouldBeFalse();
        }

        #endregion FromError Tests
    }
}
