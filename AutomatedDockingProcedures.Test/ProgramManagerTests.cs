﻿using Moq;
using NUnit.Framework;
using Shouldly;

using Sandbox.ModAPI.Ingame;

using static IngameScript.Program;

namespace AutomatedDockingProcedures.Test
{
    [TestFixture]
    public class ProgramManagerTests
    {
        #region Test Context

        private class TestContext
        {
            public string RenderedLog;

            public TestContext()
            {
                MockBackgroundWorker = new Mock<IBackgroundWorker>();

                MockConfigManager = new Mock<IConfigManager>();
                MockConfigManager
                    .Setup(x => x.MakeParseOperation())
                    .Returns(() => MockParseOperation.Object);

                MockDockingManager = new Mock<IDockingManager>();
                MockDockingManager
                    .Setup(x => x.MakeDockOperation())
                    .Returns(() => MockDockOperation.Object);
                MockDockingManager
                    .Setup(x => x.MakeUndockOperation())
                    .Returns(() => MockUndockOperation.Object);
                MockDockingManager
                    .Setup(x => x.MakeToggleOperation())
                    .Returns(() => MockToggleOperation.Object);

                MockEchoProvider = new Mock<IEchoProvider>();

                MockGridProgramRuntimeInfo = new Mock<IMyGridProgramRuntimeInfo>();

                MockLogger = new Mock<ILogger>();
                MockLogger
                    .Setup(x => x.Render())
                    .Returns(() => RenderedLog);

                Uut = new ProgramManager(
                    MockBackgroundWorker.Object,
                    MockConfigManager.Object,
                    MockDockingManager.Object,
                    MockEchoProvider.Object,
                    MockGridProgramRuntimeInfo.Object,
                    MockLogger.Object);

                MockParseOperation = new Mock<IBackgroundOperation>();

                MockDockOperation = new Mock<IBackgroundOperation>();

                MockUndockOperation = new Mock<IBackgroundOperation>();

                MockToggleOperation = new Mock<IBackgroundOperation>();
            }

            public readonly Mock<IBackgroundWorker> MockBackgroundWorker;

            public readonly Mock<IConfigManager> MockConfigManager;

            public readonly Mock<IDockingManager> MockDockingManager;

            public readonly Mock<IEchoProvider> MockEchoProvider;

            public readonly Mock<IMyGridProgramRuntimeInfo> MockGridProgramRuntimeInfo;

            public readonly Mock<ILogger> MockLogger;

            public readonly ProgramManager Uut;

            public readonly Mock<IBackgroundOperation> MockParseOperation;

            public readonly Mock<IBackgroundOperation> MockDockOperation;

            public readonly Mock<IBackgroundOperation> MockUndockOperation;

            public readonly Mock<IBackgroundOperation> MockToggleOperation;
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

            testContext.MockDockingManager
                .Invocations.ShouldBeEmpty();

            testContext.MockBackgroundWorker
                .ShouldHaveReceived(x => x.ScheduleOperation(testContext.MockParseOperation.Object));

            testContext.MockGridProgramRuntimeInfo
                .ShouldHaveReceivedSet(x => x.UpdateFrequency = It.Is<UpdateFrequency>(y => y.HasFlag(UpdateFrequency.Once)));

            testContext.MockLogger
                .ShouldNotHaveReceived(x => x.AddLine(It.IsAny<string>()));
        }

        [TestCase("dock")]
        public void Run_ArgumentIsDock_SchedulesDockOperations(string argument)
        {
            var testContext = new TestContext();

            testContext.Uut.Run(argument);

            testContext.MockConfigManager
                .Invocations.ShouldBeEmpty();

            testContext.MockDockingManager
                .ShouldHaveReceived(x => x.MakeDockOperation());

            testContext.MockBackgroundWorker
                .ShouldHaveReceived(x => x.ScheduleOperation(testContext.MockDockOperation.Object));

            testContext.MockGridProgramRuntimeInfo
                .ShouldHaveReceivedSet(x => x.UpdateFrequency = It.Is<UpdateFrequency>(y => y.HasFlag(UpdateFrequency.Once)));

            testContext.MockLogger
                .ShouldNotHaveReceived(x => x.AddLine(It.IsAny<string>()));
        }

        [TestCase("undock")]
        public void Run_ArgumentIsUndock_SchedulesUndockOperations(string argument)
        {
            var testContext = new TestContext();

            testContext.Uut.Run(argument);

            testContext.MockConfigManager
                .Invocations.ShouldBeEmpty();

            testContext.MockDockingManager
                .ShouldHaveReceived(x => x.MakeUndockOperation());

            testContext.MockBackgroundWorker
                .ShouldHaveReceived(x => x.ScheduleOperation(testContext.MockUndockOperation.Object));

            testContext.MockGridProgramRuntimeInfo
                .ShouldHaveReceivedSet(x => x.UpdateFrequency = It.Is<UpdateFrequency>(y => y.HasFlag(UpdateFrequency.Once)));

            testContext.MockLogger
                .ShouldNotHaveReceived(x => x.AddLine(It.IsAny<string>()));
        }

        [TestCase("toggle")]
        public void Run_ArgumentIsToggle_SchedulesToggleOperations(string argument)
        {
            var testContext = new TestContext();

            testContext.Uut.Run(argument);

            testContext.MockConfigManager
                .Invocations.ShouldBeEmpty();

            testContext.MockDockingManager
                .ShouldHaveReceived(x => x.MakeToggleOperation());

            testContext.MockBackgroundWorker
                .ShouldHaveReceived(x => x.ScheduleOperation(testContext.MockToggleOperation.Object));

            testContext.MockGridProgramRuntimeInfo
                .ShouldHaveReceivedSet(x => x.UpdateFrequency = It.Is<UpdateFrequency>(y => y.HasFlag(UpdateFrequency.Once)));

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

            testContext.MockDockingManager
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

            testContext.MockDockingManager
                .Invocations.ShouldBeEmpty();

            testContext.MockGridProgramRuntimeInfo
                .Invocations.ShouldBeEmpty();
        }

        [TestCase("invalid-argument", "RenderedLog1")]
        [TestCase("reload",           "RenderedLog2")]
        [TestCase("dock",             "RenderedLog3")]
        [TestCase("undock",           "RenderedLog4")]
        [TestCase("toggle",           "RenderedLog5")]
        [TestCase("run",              "RenderedLog6")]
        [TestCase("",                 "RenderedLog7")]
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
