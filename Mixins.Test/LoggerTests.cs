using System;
using System.Collections.Generic;
using System.Linq;

using Moq;
using NUnit.Framework;
using Shouldly;

using static IngameScript.Program;

namespace Mixins.Test
{
    [TestFixture]
    public class LoggerTests
    {
        #region Test Context

        private class TestContext
        {
            public DateTime Now;

            public int MaxLogSize;

            public TestContext()
            {
                Now = DateTime.Now;
                MaxLogSize = 100;

                MockDateTimeProvider = new Mock<IDateTimeProvider>();
                MockDateTimeProvider
                    .Setup(x => x.Now)
                    .Returns(() => Now);

                MockLoggerSettingsProvider = new Mock<ILoggerSettingsProvider>();
                MockLoggerSettingsProvider
                    .Setup(x => x.Settings)
                    .Returns(() => new LoggerSettings()
                    {
                        MaxLogSize = MaxLogSize
                    });

                Uut = new Logger(
                    MockDateTimeProvider.Object,
                    MockLoggerSettingsProvider.Object);
            }

            public Mock<IDateTimeProvider> MockDateTimeProvider;

            public Mock<ILoggerSettingsProvider> MockLoggerSettingsProvider;

            public Logger Uut;

            public string[] AddExistingLines(int lineCount)
            {
                var existingLines = Enumerable.Range(1, lineCount)
                    .Select(x => $"Line #{x}")
                    .ToArray();

                foreach (var existingLine in existingLines)
                    Uut.AddLine(existingLine);

                return existingLines;
            }
        }

        #endregion Test Context

        #region AddLine() Tests

        [TestCase("")]
        [TestCase("log line")]
        public void AddLine_MaxLineCountIs0_DoesNothing(string line)
        {
            var testContext = new TestContext()
            {
                MaxLogSize = 0
            };

            testContext.Uut.AddLine(line);

            testContext.Uut.LogLines.ShouldBeEmpty();
        }

        [TestCase(1,  0,  "")]
        [TestCase(1,  0,  "log line")]
        [TestCase(1,  1,  "log line")]
        [TestCase(2,  0,  "log line")]
        [TestCase(2,  1,  "log line")]
        [TestCase(2,  2,  "log line")]
        [TestCase(10, 0,  "log line")]
        [TestCase(10, 5,  "log line")]
        [TestCase(10, 9,  "log line")]
        [TestCase(10, 10, "log line")]
        public void AddLine_MaxLogSizeIsGreaterThan0_AddsLineToFrontAndRemovesLinesFromBackIfNeeded(int maxLogSize, int lineCount, string line)
        {
            var testContext = new TestContext()
            {
                MaxLogSize = maxLogSize
            };

            var existingLines = testContext.AddExistingLines(lineCount);

            testContext.Uut.AddLine(line);

            testContext.Uut.LogLines.ShouldBe(
                existingLines
                    .Reverse()
                    .Prepend(line)
                    .Take(maxLogSize),
                ignoreOrder: false);
        }

        #endregion AddLine() Tests

        #region Clear() Tests

        [TestCase(0,  0)]
        [TestCase(0,  1)]
        [TestCase(0,  5)]
        [TestCase(0,  10)]
        [TestCase(1,  0)]
        [TestCase(1,  1)]
        [TestCase(1,  5)]
        [TestCase(1,  10)]
        [TestCase(10, 0)]
        [TestCase(10, 1)]
        [TestCase(10, 5)]
        [TestCase(10, 10)]
        public void Clear_Always_ClearsLines(int maxLogSize, int lineCount)
        {
            var testContext = new TestContext()
            {
                MaxLogSize = maxLogSize
            };

            testContext.AddExistingLines(lineCount);

            testContext.Uut.Clear();

            testContext.Uut.LogLines.ShouldBeEmpty();
        }

        #endregion Clear() Tests

        #region Render() Tests

        [TestCase(0,  0)]
        [TestCase(1,  0)]
        [TestCase(1,  1)]
        [TestCase(10, 0)]
        [TestCase(10, 5)]
        [TestCase(10, 10)]
        public void Render_Always_StartsWithNow(int maxLogSize, int lineCount)
        {
            var testContext = new TestContext()
            {
                MaxLogSize = maxLogSize
            };

            testContext.AddExistingLines(lineCount);

            var result = testContext.Uut.Render();

            result.ShouldStartWith(testContext.Now.ToString(Logger.DateTimeFormat));
        }

        [TestCase(0,  0)]
        [TestCase(0,  1)]
        [TestCase(0,  5)]
        [TestCase(1,  0)]
        [TestCase(1,  1)]
        [TestCase(1,  5)]
        [TestCase(10, 0)]
        [TestCase(10, 5)]
        [TestCase(10, 10)]
        [TestCase(10, 15)]
        public void Render_Always_IncludesAllCurrentLinesOrderedByAge(int maxLogSize, int lineCount)
        {
            var testContext = new TestContext()
            {
                MaxLogSize = maxLogSize
            };

            var existingLines = testContext.AddExistingLines(lineCount);

            var result = testContext.Uut.Render();

            result.ShouldContain(string.Join("\n", existingLines
                .Reverse()
                .Take(maxLogSize)));
        }

        #endregion Render() Tests
    }
}
