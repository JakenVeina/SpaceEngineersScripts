using System;
using System.Linq;

using Moq;
using NUnit.Framework;
using Shouldly;

using Sandbox.ModAPI.Ingame;

using static IngameScript.Program;

using Mixins.Test.Common;

namespace AutomatedDockingProcedures.Test
{
    [TestFixture]
    public class ConnectorCollectionHandlerTests
    {
        #region Test Context

        private class TestContext
        {
            public int ConnectorCount;

            public TestContext()
            {
                MockConnectorManager = new Mock<IConnectorManager>();
                MockConnectorManager
                    .Setup(x => x.Blocks.Count)
                    .Returns(() => ConnectorCount);

                MockLogger = new Mock<ILogger>();

                Uut = new ConnectorCollectionHandler(
                    MockConnectorManager.Object,
                    MockLogger.Object);

                MockBackgroundWorker = new FakeBackgroundWorker();
            }

            public readonly Mock<IConnectorManager> MockConnectorManager;

            public readonly Mock<ILogger> MockLogger;

            public readonly ConnectorCollectionHandler Uut;

            public readonly FakeBackgroundWorker MockBackgroundWorker;
        }

        #endregion Test Context

        #region OnStarting() Tests

        [Test]
        public void OnStarting_Always_ClearsConnectors()
        {
            var testContext = new TestContext();

            testContext.Uut.OnStarting();

            testContext.MockConnectorManager
                .ShouldHaveReceived(x => x.ClearBlocks());
        }

        #endregion OnStarting() Tests

        #region MakeCollectBlockOperation Tests

        [Test]
        public void MakeCollectBlockOperation_BlockIsNotConnector_IgnoresBlock()
        {
            var testContext = new TestContext();

            var mockBlock = new Mock<IMyTerminalBlock>();

            var result = testContext.Uut.MakeCollectBlockOperation(mockBlock.Object);
            result.ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            testContext.MockBackgroundWorker.MockSubOperationScheduler
                .ShouldNotHaveReceived(x => x(It.IsAny<IBackgroundOperation>()));

            testContext.MockConnectorManager
                .ShouldNotHaveReceived(x => x.AddBlock(It.IsAny<IMyShipConnector>()));

            result.Result.IsIgnored.ShouldBeTrue();
        }

        [Test]
        public void MakeCollectBlockOperation_BlockIsConnector_AddsConnector()
        {
            var testContext = new TestContext();

            var mockBlock = new Mock<IMyShipConnector>();

            var result = testContext.Uut.MakeCollectBlockOperation(mockBlock.Object);
            result.ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            testContext.MockBackgroundWorker.MockSubOperationScheduler
                .ShouldNotHaveReceived(x => x(It.IsAny<IBackgroundOperation>()));

            testContext.MockConnectorManager
                .ShouldHaveReceived(x => x.AddBlock(mockBlock.Object));

            result.Result.IsSuccess.ShouldBeTrue();
        }

        [Test]
        public void MakeCollectBlockOperation_OperationIsDisposed_RecyclesOperation()
        {
            var testContext = new TestContext();

            var mockBlock = new Mock<IMyShipConnector>();

            var result = testContext.Uut.MakeCollectBlockOperation(mockBlock.Object);
            result.ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            var mockBlockInvocations = mockBlock
                .Invocations.ToArray();

            var mockDoorManagerInvocations = testContext.MockConnectorManager
                .Invocations.ToArray();

            result.ShouldBeAssignableTo<IDisposable>();
            (result as IDisposable).Dispose();

            mockBlock
                .Invocations.Clear();

            testContext.MockConnectorManager
                .Invocations.Clear();

            var secondMockBlock = new Mock<IMyShipConnector>();

            testContext.Uut.MakeCollectBlockOperation(secondMockBlock.Object)
                .ShouldBeSameAs(result);

            result.ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            secondMockBlock.Invocations.Count.ShouldBe(mockBlockInvocations.Length);
            foreach (var i in Enumerable.Range(0, mockBlockInvocations.Length))
                secondMockBlock.Invocations[i].ShouldBe(mockBlockInvocations[i]);

            testContext.MockConnectorManager.Invocations.Count.ShouldBe(mockDoorManagerInvocations.Length);
            foreach (var i in Enumerable.Range(0, mockDoorManagerInvocations.Length))
                testContext.MockConnectorManager.Invocations[i].ShouldBe(mockDoorManagerInvocations[i]);
        }

        #endregion MakeCollectBlockOperation Tests

        #region OnCompleted() Tests

        [Test]
        public void OnCompleted_ConnectorCountIs0_DoesNotLogConnectorCount()
        {
            var testContext = new TestContext()
            {
                ConnectorCount = 0
            };

            testContext.Uut.OnCompleted();

            testContext.MockLogger
                .ShouldNotHaveReceived(x => x.AddLine(It.IsAny<string>()));
        }

        [TestCase(1)]
        [TestCase(10)]
        public void OnCompleted_ConnectorCountIsGreaterThan0_LogsConnectorCount(int connectorCount)
        {
            var testContext = new TestContext()
            {
                ConnectorCount = connectorCount
            };

            testContext.Uut.OnCompleted();

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.Contains(connectorCount.ToString()))));
        }

        #endregion OnCompleted() Tests
    }
}
