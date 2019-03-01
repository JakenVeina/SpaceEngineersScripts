using Moq;
using NUnit.Framework;
using Shouldly;

using static IngameScript.Program;

namespace AutomatedDockingProcedures.Test
{
    [TestFixture]
    public class ProgramReloadHandlerTests
    {
        #region Test Context

        private class TestContext
        {
            public TestContext()
            {
                MockBackgroundWorker = new Mock<IBackgroundWorker>();

                MockBlockCollectionManager = new Mock<IBlockCollectionManager>();
                MockBlockCollectionManager
                    .Setup(x => x.MakeCollectBlocksOperation())
                    .Returns(() => MockCollectBlocksOperation.Object);

                Uut = new ProgramReloadHandler(
                    MockBackgroundWorker.Object,
                    MockBlockCollectionManager.Object);

                MockCollectBlocksOperation = new Mock<IBackgroundOperation>();
            }

            public readonly Mock<IBackgroundWorker> MockBackgroundWorker;

            public readonly Mock<IBlockCollectionManager> MockBlockCollectionManager;

            public readonly ProgramReloadHandler Uut;

            public readonly Mock<IBackgroundOperation> MockCollectBlocksOperation;
        }

        #endregion Test Context

        #region OnStarting() Tests

        [Test]
        public void OnStarting_Always_SchedulesCollectBlocks()
        {
            var testContext = new TestContext();

            testContext.Uut.OnStarting();

            testContext.MockBlockCollectionManager
                .ShouldHaveReceived(x => x.MakeCollectBlocksOperation(), 1);

            testContext.MockBackgroundWorker
                .ShouldHaveReceived(x => x.ScheduleOperation(testContext.MockCollectBlocksOperation.Object));
        }

        #endregion OnStarting() Tests

        #region OnParsing() Tests

        [TestCase("option")]
        [TestCase("option", "param")]
        public void OnParsing_Always_IgnoresLine(params string[] configLinePieces)
        {
            var testContext = new TestContext();

            var result = testContext.Uut.OnParsing(new ConfigLine(configLinePieces));

            result.IsIgnored.ShouldBeTrue();

            testContext.MockBlockCollectionManager
                .Invocations.ShouldBeEmpty();

            testContext.MockBackgroundWorker
                .Invocations.ShouldBeEmpty();
        }

        #endregion OnParsing() Tests

        #region OnCompleted() Tests

        [Test]
        public void OnCompleted_Always_DoesNothing()
        {
            var testContext = new TestContext();

            testContext.Uut.OnCompleted();

            testContext.MockBlockCollectionManager
                .Invocations.ShouldBeEmpty();

            testContext.MockBackgroundWorker
                .Invocations.ShouldBeEmpty();
        }

        #endregion OnCompleted() Tests
    }
}
