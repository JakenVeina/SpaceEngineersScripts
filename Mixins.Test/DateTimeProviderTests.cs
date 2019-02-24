using System;

using NUnit.Framework;
using Shouldly;

using static IngameScript.Program;

namespace Mixins.Test
{
    [TestFixture]
    public class DateTimeProviderTests
    {
        #region Now Tests

        [Test]
        public void Now_Always_ReturnsNow()
        {
            var uut = new DateTimeProvider();

            uut.Now.ShouldBe(DateTime.Now, TimeSpan.FromMinutes(1));
        }

        #endregion Now Tests
    }
}
