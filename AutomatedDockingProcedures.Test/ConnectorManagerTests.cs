using System.Linq;

using Moq;
using NUnit.Framework;
using Shouldly;

using Sandbox.ModAPI.Ingame;

using static IngameScript.Program;

namespace AutomatedDockingProcedures.Test
{
    [TestFixture]
    public class ConnectorManagerTests
    {
        #region Test Context

        private class TestContext
        {
            public TestContext()
            {
                Uut = new ConnectorManager();
            }

            public readonly ConnectorManager Uut;
        }

        #endregion Test Context

        #region AddConnector() Tests

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void AddConnector_Always_AddsConnector(int existingConnectorCount)
        {
            var testContext = new TestContext();

            var mockExistingConnectors = Enumerable.Repeat(0, existingConnectorCount)
                .Select(_ => new Mock<IMyShipConnector>())
                .ToArray();
            
            foreach(var mockExistingConnector in mockExistingConnectors)
                testContext.Uut.AddConnector(mockExistingConnector.Object);

            var mockConnector = new Mock<IMyShipConnector>();

            testContext.Uut.AddConnector(mockConnector.Object);

            testContext.Uut.Connectors.ShouldContain(mockConnector.Object);
        }

        #endregion AddConnector() Tests

        #region ClearConnectors() Tests

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void ClearConnectors_Always_ClearsConnectors(int connectorCount)
        {
            var testContext = new TestContext();

            var mockConnectors = Enumerable.Repeat(0, connectorCount)
                .Select(_ => new Mock<IMyShipConnector>())
                .ToArray();

            foreach (var mockConnector in mockConnectors)
                testContext.Uut.AddConnector(mockConnector.Object);

            testContext.Uut.ClearConnectors();

            testContext.Uut.Connectors.ShouldBeEmpty();
        }

        #endregion ClearConnectors() Tests

        #region MakeConnectOperation() Tests

        [Test]
        public void MakeConnectOperation_ConnectorsIsEmpty_CompletesImmediately()
        {
            var testContext = new TestContext();

            testContext.Uut.MakeConnectOperation()
                .ShouldRunToCompletionIn(1);
        }

        [TestCase(1)]
        [TestCase(10)]
        public void MakeConnectOperation_ConnectorsIsNotEmpty_ConnectsEachConnector(int connectorCount)
        {
            var testContext = new TestContext();

            var mockConnectors = Enumerable.Repeat(0, connectorCount)
                .Select(_ => new Mock<IMyShipConnector>())
                .ToArray();

            foreach (var mockConnector in mockConnectors)
                testContext.Uut.AddConnector(mockConnector.Object);

            testContext.Uut.MakeConnectOperation()
                .ShouldRunToCompletionIn(connectorCount);

            mockConnectors.ForEach(mockConnector =>
                mockConnector.ShouldHaveReceived(x => x.Connect()));
        }

        [TestCase(1)]
        [TestCase(10)]
        public void MakeConnectOperation_OperationIsDisposed_RecyclesOperation(int connectorCount)
        {
            var testContext = new TestContext();

            var mockConnectors = Enumerable.Repeat(0, connectorCount)
                .Select(_ => new Mock<IMyShipConnector>())
                .ToArray();

            foreach (var mockConnector in mockConnectors)
                testContext.Uut.AddConnector(mockConnector.Object);

            var result = testContext.Uut.MakeConnectOperation();
            result.ShouldRunToCompletionIn(connectorCount);

            var mockConnectorInvocations = mockConnectors
                .Select(x => x.Invocations.ToArray())
                .ToArray();

            mockConnectors.ForEach(x => x
                .Invocations.Clear());

            testContext.Uut.MakeConnectOperation()
                .ShouldBeSameAs(result);

            result.ShouldRunToCompletionIn(connectorCount);

            foreach(var i in Enumerable.Range(0, mockConnectors.Length))
            {
                mockConnectors[i].Invocations.Count.ShouldBe(mockConnectorInvocations[i].Length);
                foreach (var j in Enumerable.Range(0, mockConnectors[i].Invocations.Count))
                    mockConnectors[i].Invocations[j].ShouldBe(mockConnectorInvocations[i][j]);
            }
        }

        #endregion MakeConnectOperation() Tests

        #region MakeDisconnectOperation() Tests

        [Test]
        public void MakeDisconnectOperation_ConnectorsIsEmpty_CompletesImmediately()
        {
            var testContext = new TestContext();

            testContext.Uut.MakeDisconnectOperation()
                .ShouldRunToCompletionIn(1);
        }

        [TestCase(1)]
        [TestCase(10)]
        public void MakeDisconnectOperation_ConnectorsIsNotEmpty_DisconnectsEachConnector(int connectorCount)
        {
            var testContext = new TestContext();

            var mockConnectors = Enumerable.Repeat(0, connectorCount)
                .Select(_ => new Mock<IMyShipConnector>())
                .ToArray();

            foreach (var mockConnector in mockConnectors)
                testContext.Uut.AddConnector(mockConnector.Object);

            testContext.Uut.MakeDisconnectOperation()
                .ShouldRunToCompletionIn(connectorCount);

            mockConnectors.ForEach(mockConnector =>
                mockConnector.ShouldHaveReceived(x => x.Disconnect()));
        }

        [TestCase(1)]
        [TestCase(10)]
        public void MakeDisconnectOperation_OperationIsDisposed_RecyclesOperation(int connectorCount)
        {
            var testContext = new TestContext();

            var mockConnectors = Enumerable.Repeat(0, connectorCount)
                .Select(_ => new Mock<IMyShipConnector>())
                .ToArray();

            foreach (var mockConnector in mockConnectors)
                testContext.Uut.AddConnector(mockConnector.Object);

            var result = testContext.Uut.MakeDisconnectOperation();
            result.ShouldRunToCompletionIn(connectorCount);

            var mockConnectorInvocations = mockConnectors
                .Select(x => x.Invocations.ToArray())
                .ToArray();

            mockConnectors.ForEach(x => x
                .Invocations.Clear());

            testContext.Uut.MakeDisconnectOperation()
                .ShouldBeSameAs(result);

            result.ShouldRunToCompletionIn(connectorCount);

            foreach (var i in Enumerable.Range(0, mockConnectors.Length))
            {
                mockConnectors[i].Invocations.Count.ShouldBe(mockConnectorInvocations[i].Length);
                foreach (var j in Enumerable.Range(0, mockConnectors[i].Invocations.Count))
                    mockConnectors[i].Invocations[j].ShouldBe(mockConnectorInvocations[i][j]);
            }
        }

        #endregion MakeDisconnectOperation() Tests
    }
}
