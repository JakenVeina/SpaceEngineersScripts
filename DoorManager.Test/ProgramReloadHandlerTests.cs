using System;

using Moq;
using NUnit.Framework;
using Shouldly;

using static IngameScript.Program;

namespace DoorManager.Test
{
    [TestFixture]
    public class ProgramReloadHandlerTests
    {
        #region Test Context

        private class TestContext
        {
            public TimeSpan ManageInterval;

            public TestContext()
            {
                MockBackgroundWorker = new Mock<IBackgroundWorker>();

                MockBlockCollectionManager = new Mock<IBlockCollectionManager>();
                MockBlockCollectionManager
                    .Setup(x => x.MakeCollectBlocksOperation())
                    .Returns(() => MockCollectBlocksOperation.Object);

                MockDoorManager = new Mock<IDoorManager>();

                MockProgramSettingsProvider = new Mock<IProgramSettingsProvider>();
                MockProgramSettingsProvider
                    .Setup(x => x.Settings)
                    .Returns(() => new ProgramSettings()
                    {
                        ManageInterval = ManageInterval
                    });

                Uut = new ProgramReloadHandler(
                    MockBackgroundWorker.Object,
                    MockBlockCollectionManager.Object,
                    MockDoorManager.Object,
                    MockProgramSettingsProvider.Object);

                MockCollectBlocksOperation = new Mock<IBackgroundOperation>();
            }

            public readonly Mock<IBackgroundWorker> MockBackgroundWorker;

            public readonly Mock<IBlockCollectionManager> MockBlockCollectionManager;

            public readonly Mock<IDoorManager> MockDoorManager;

            public readonly Mock<IProgramSettingsProvider> MockProgramSettingsProvider;

            public readonly ProgramReloadHandler Uut;

            public readonly Mock<IBackgroundOperation> MockCollectBlocksOperation;
        }

        #endregion Test Context

        #region OnStarting() Tests

        [Test]
        public void OnStarting_Always_ClearsRecurringOperationsAndSchedulesBlockCollection()
        {
            var testContext = new TestContext();

            testContext.Uut.OnStarting();

            testContext.MockBackgroundWorker
                .ShouldHaveReceived(x => x.ClearRecurringOperations());

            testContext.MockBlockCollectionManager
                .ShouldHaveReceived(x => x.MakeCollectBlocksOperation(), 1);

            testContext.MockBackgroundWorker
                .ShouldHaveReceived(x => x.ScheduleOperation(testContext.MockCollectBlocksOperation.Object));
        }

        #endregion OnStarting() Tests

        #region OnParsing() Tests

        [TestCase()]
        [TestCase("option")]
        [TestCase("option", "param")]
        [TestCase("option", "param", "value")]
        public void OnParsing_Always_IgnoresLine(params string[] linePieces)
        {
            var testContext = new TestContext();

            var configLine = new ConfigLine(linePieces);

            var result = testContext.Uut.OnParsing(configLine);

            result.IsIgnored.ShouldBeTrue();

            testContext.MockBackgroundWorker
                .Invocations.ShouldBeEmpty();

            testContext.MockBlockCollectionManager
                .Invocations.ShouldBeEmpty();

            testContext.MockDoorManager
                .Invocations.ShouldBeEmpty();

            testContext.MockProgramSettingsProvider
                .Invocations.ShouldBeEmpty();
        }

        #endregion OnParsing() Tests

        #region OnCompleted() Tests

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        public void OnCompleted_Always_RegistersRecurringManageDoorsOperation(int manageIntervalMilliseconds)
        {
            var testContext = new TestContext()
            {
                ManageInterval = TimeSpan.FromMilliseconds(manageIntervalMilliseconds)
            };

            testContext.Uut.OnCompleted();

            testContext.MockDoorManager
                .Invocations.ShouldBeEmpty();

            testContext.MockBackgroundWorker
                .ShouldHaveReceived(x => x.RegisterRecurringOperation(
                    testContext.ManageInterval,
                    testContext.MockDoorManager.Object.MakeManageDoorsOperation));
        }

        #endregion OnCompleted() Tests
    }
}
