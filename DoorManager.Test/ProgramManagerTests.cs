using Sandbox.ModAPI.Ingame;

using Moq;
using NUnit.Framework;
using Shouldly;

using static IngameScript.Program;

namespace DoorManager.Test
{
    [TestFixture]
    public class ProgramManagerTests
    {
        #region Test Context

        private class TestContext
        {
            public int DoorCount;

            public string RenderedLog;

            public TestContext()
            {
                MockBackgroundWorker = new Mock<IBackgroundWorker>();

                MockConfigManager = new Mock<IConfigManager>();
                MockConfigManager
                    .Setup(x => x.MakeParseOperation())
                    .Returns(() => MockParseOperation.Object);

                MockDoorManager = new Mock<IDoorManager>();
                MockDoorManager
                    .Setup(x => x.DoorCount)
                    .Returns(() => DoorCount);

                MockEchoProvider = new Mock<IEchoProvider>();

                MockGridProgramRuntimeInfo = new Mock<IMyGridProgramRuntimeInfo>();

                MockLogger = new Mock<ILogger>();
                MockLogger
                    .Setup(x => x.Render())
                    .Returns(() => RenderedLog);

                Uut = new ProgramManager(
                    MockBackgroundWorker.Object,
                    MockConfigManager.Object,
                    MockDoorManager.Object,
                    MockEchoProvider.Object,
                    MockGridProgramRuntimeInfo.Object,
                    MockLogger.Object);

                MockParseOperation = new Mock<IBackgroundOperation>();
            }

            public readonly Mock<IBackgroundWorker> MockBackgroundWorker;

            public readonly Mock<IConfigManager> MockConfigManager;

            public readonly Mock<IDoorManager> MockDoorManager;

            public readonly Mock<IEchoProvider> MockEchoProvider;

            public readonly Mock<IMyGridProgramRuntimeInfo> MockGridProgramRuntimeInfo;

            public readonly Mock<ILogger> MockLogger;

            public readonly ProgramManager Uut;

            public readonly Mock<IBackgroundOperation> MockParseOperation;
        }

        #endregion Test Context

        #region Run() Tests

        [TestCase("reload")]
        public void Run_ArgumentIsReload_SchedulesConfigParse(string argument)
        {
            var testContext = new TestContext();

            testContext.Uut.Run(argument);

            testContext.MockConfigManager
                .ShouldHaveReceived(x => x.MakeParseOperation(), 1);

            testContext.MockBackgroundWorker
                .ShouldHaveReceived(x => x.ScheduleOperation(testContext.MockParseOperation.Object));

            testContext.MockGridProgramRuntimeInfo
                .ShouldHaveReceivedSet(x => x.UpdateFrequency = It.Is<UpdateFrequency>(y => y.HasFlag(UpdateFrequency.Once)));

            testContext.MockDoorManager
                .Invocations.ShouldBeEmpty();

            testContext.MockLogger
                .ShouldNotHaveReceived(x => x.AddLine(It.IsAny<string>()));
        }

        [TestCase("lockdown")]
        public void Run_ArgumentIsLockdown_EnablesLockdown(string argument)
        {
            var testContext = new TestContext();

            testContext.Uut.Run(argument);

            testContext.MockDoorManager
                .ShouldHaveReceivedSet(x => x.IsLockdownEnabled = true);

            testContext.MockGridProgramRuntimeInfo
                .ShouldHaveReceivedSet(x => x.UpdateFrequency = It.Is<UpdateFrequency>(y => y.HasFlag(UpdateFrequency.Once)));

            testContext.MockBackgroundWorker
                .Invocations.ShouldBeEmpty();

            testContext.MockConfigManager
                .Invocations.ShouldBeEmpty();

            testContext.MockLogger
                .ShouldNotHaveReceived(x => x.AddLine(It.IsAny<string>()));
        }

        [TestCase("release")]
        public void Run_ArgumentIsRelease_DisablesLockdown(string argument)
        {
            var testContext = new TestContext();

            testContext.Uut.Run(argument);

            testContext.MockDoorManager
                .ShouldHaveReceivedSet(x => x.IsLockdownEnabled = false);

            testContext.MockGridProgramRuntimeInfo
                .ShouldHaveReceivedSet(x => x.UpdateFrequency = It.Is<UpdateFrequency>(y => y.HasFlag(UpdateFrequency.Once)));

            testContext.MockBackgroundWorker
                .Invocations.ShouldBeEmpty();

            testContext.MockConfigManager
                .Invocations.ShouldBeEmpty();

            testContext.MockLogger
                .ShouldNotHaveReceived(x => x.AddLine(It.IsAny<string>()));
        }

        [TestCase("stats", 0)]
        [TestCase("stats", 1)]
        [TestCase("stats", 10)]
        [TestCase("stats", 100)]
        public void Run_ArgumentIsStats_LogsStats(string argument, int doorCount)
        {
            var testContext = new TestContext()
            {
                DoorCount = doorCount
            };

            testContext.Uut.Run(argument);

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.Contains(doorCount.ToString()))));

            testContext.MockBackgroundWorker
                .Invocations.ShouldBeEmpty();

            testContext.MockConfigManager
                .Invocations.ShouldBeEmpty();

            testContext.MockGridProgramRuntimeInfo
                .Invocations.ShouldBeEmpty();
        }

        [TestCase("stop")]
        public void Run_ArgumentIsStop_CancelsUpdates(string argument)
        {
            var testContext = new TestContext();

            testContext.Uut.Run(argument);

            testContext.MockGridProgramRuntimeInfo
                .ShouldHaveReceivedSet(x => x.UpdateFrequency = UpdateFrequency.None);

            testContext.MockBackgroundWorker
                .Invocations.ShouldBeEmpty();

            testContext.MockConfigManager
                .Invocations.ShouldBeEmpty();

            testContext.MockDoorManager
                .Invocations.ShouldBeEmpty();

            testContext.MockLogger
                .ShouldNotHaveReceived(x => x.AddLine(It.IsAny<string>()));
        }

        [TestCase("")]
        [TestCase("run")]
        public void Run_ArgumentIsRun_ExecutesOperations(string argument)
        {
            var testContext = new TestContext();

            testContext.Uut.Run(argument);

            testContext.MockBackgroundWorker
                .ShouldHaveReceived(x => x.ExecuteOperations());

            testContext.MockConfigManager
                .Invocations.ShouldBeEmpty();

            testContext.MockDoorManager
                .Invocations.ShouldBeEmpty();

            testContext.MockGridProgramRuntimeInfo
                .Invocations.ShouldBeEmpty();

            testContext.MockLogger
                .ShouldNotHaveReceived(x => x.AddLine(It.IsAny<string>()));
        }

        [TestCase("invalid-argument")]
        public void Run_ArgumentIsInvalid_LogsError(string argument)
        {
            var testContext = new TestContext();

            testContext.Uut.Run(argument);

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.Contains(argument))));

            testContext.MockBackgroundWorker
                .Invocations.ShouldBeEmpty();

            testContext.MockConfigManager
                .Invocations.ShouldBeEmpty();

            testContext.MockDoorManager
                .Invocations.ShouldBeEmpty();

            testContext.MockGridProgramRuntimeInfo
                .Invocations.ShouldBeEmpty();
        }

        [TestCase("invalid-argument", "RenderedLog1")]
        [TestCase("reload",           "RenderedLog2")]
        [TestCase("lockdown",         "RenderedLog3")]
        [TestCase("release",          "RenderedLog4")]
        [TestCase("stats",            "RenderedLog5")]
        [TestCase("stop",             "RenderedLog6")]
        [TestCase("run",              "RenderedLog7")]
        [TestCase("",                 "RenderedLog8")]
        public void Run_Always_EchoesLog(string argument, string renderedLog)
        {
            var testContext = new TestContext()
            {
                RenderedLog = renderedLog
            };

            testContext.Uut.Run(argument);

            testContext.MockEchoProvider
                .ShouldHaveReceived(x => x.Echo(renderedLog), 1);
        }

        #endregion Run() Tests
    }
}
