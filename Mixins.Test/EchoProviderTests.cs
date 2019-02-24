using System;

using Moq;
using NUnit.Framework;
using Shouldly;

using static IngameScript.Program;

namespace Mixins.Test
{
    [TestFixture]
    public class EchoProviderTests
    {
        #region Echo Tests

        [TestCase(null)]
        [TestCase("")]
        [TestCase("This is a test")]
        [TestCase("This is only a test")]
        public void Echo_Always_InvokesEchoAction(string text)
        {
            var echoActionMock = new Mock<Action<string>>();

            var uut = new EchoProvider(echoActionMock.Object);

            uut.Echo(text);

            echoActionMock.ShouldHaveReceived(x => x(text));
        }

        #endregion Echo Tests
    }
}
