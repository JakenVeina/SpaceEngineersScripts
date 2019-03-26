using System;
using System.Linq;
using System.Collections.Generic;

using Moq;
using NUnit.Framework;
using Shouldly;

using Sandbox.ModAPI.Ingame;

using static IngameScript.Program;
using Mixins.Test.Common;

namespace AutomatedDockingProcedures.Test
{
    [TestFixture]
    public class DockingManagerTests
    {
        #region Test Context

        private class TestContext
        {
            public TestContext()
            {
                MockLogger = new Mock<ILogger>();

                MockBatteryBlockManager = new Mock<IBatteryBlockManager>();
                MockBatteryBlockManager
                    .Setup(x => x.MakeOnUndockingOperation())
                    .Returns(() => MockBatteryBlocksOnUndockingOperation.Object);
                MockBatteryBlockManager
                    .Setup(x => x.MakeOnDockingOperation())
                    .Returns(() => MockBatteryBlocksOnDockingOperation.Object);

                MockConnectorManager = new Mock<IConnectorManager>();
                MockConnectorManager
                    .Setup(x => x.Blocks)
                    .Returns(() => _connectors);
                MockConnectorManager
                    .Setup(x => x.MakeOnDockingOperation())
                    .Returns(() => MockConnectorsOnDockingOperation.Object);
                MockConnectorManager
                    .Setup(x => x.MakeOnUndockingOperation())
                    .Returns(() => MockConnectorsOnUndockingOperation.Object);

                MockFunctionalBlockManager = new Mock<IFunctionalBlockManager>();
                MockFunctionalBlockManager
                    .Setup(x => x.MakeOnUndockingOperation())
                    .Returns(() => MockFunctionalBlocksOnDockingOperation.Object);
                MockFunctionalBlockManager
                    .Setup(x => x.MakeOnDockingOperation())
                    .Returns(() => MockFunctionalBlocksOnUndockingOperation.Object);

                MockGasTankManager = new Mock<IGasTankManager>();
                MockGasTankManager
                    .Setup(x => x.MakeOnDockingOperation())
                    .Returns(() => MockGasTanksOnDockingOperation.Object);
                MockGasTankManager
                    .Setup(x => x.MakeOnUndockingOperation())
                    .Returns(() => MockGasTanksOnUndockingOperation.Object);

                MockLandingGearManager = new Mock<ILandingGearManager>();
                MockLandingGearManager
                    .Setup(x => x.MakeOnDockingOperation())
                    .Returns(() => MockLandingGearsOnDockingOperation.Object);
                MockLandingGearManager
                    .Setup(x => x.MakeOnUndockingOperation())
                    .Returns(() => MockLandingGearsOnUndockingOperation.Object);

                Uut = new DockingManager(
                    MockLogger.Object,
                    MockBatteryBlockManager.Object,
                    MockConnectorManager.Object,
                    MockFunctionalBlockManager.Object,
                    MockGasTankManager.Object,
                    MockLandingGearManager.Object);

                MockBackgroundWorker = new FakeBackgroundWorker();

                _mockConnectors = new List<Mock<IMyShipConnector>>();
                _connectors = new List<IMyShipConnector>();

                MockBatteryBlocksOnDockingOperation = MakeFakeBackgroundOperation();

                MockBatteryBlocksOnUndockingOperation = MakeFakeBackgroundOperation();

                MockConnectorsOnDockingOperation = MakeFakeBackgroundOperation();

                MockConnectorsOnUndockingOperation = MakeFakeBackgroundOperation();

                MockFunctionalBlocksOnDockingOperation = MakeFakeBackgroundOperation();

                MockFunctionalBlocksOnUndockingOperation = MakeFakeBackgroundOperation();

                MockGasTanksOnDockingOperation = MakeFakeBackgroundOperation();

                MockGasTanksOnUndockingOperation = MakeFakeBackgroundOperation();

                MockLandingGearsOnDockingOperation = MakeFakeBackgroundOperation();

                MockLandingGearsOnUndockingOperation = MakeFakeBackgroundOperation();
            }

            public readonly DockingManager Uut;

            public readonly Mock<ILogger> MockLogger;

            public IReadOnlyList<Mock<IMyShipConnector>> MockConnectors
                => _mockConnectors;
            private readonly List<Mock<IMyShipConnector>> _mockConnectors;
            private readonly List<IMyShipConnector> _connectors;

            public readonly Mock<IBatteryBlockManager> MockBatteryBlockManager;

            public readonly Mock<IConnectorManager> MockConnectorManager;

            public readonly Mock<IFunctionalBlockManager> MockFunctionalBlockManager;

            public readonly Mock<IGasTankManager> MockGasTankManager;

            public readonly Mock<ILandingGearManager> MockLandingGearManager;

            public readonly FakeBackgroundWorker MockBackgroundWorker;

            public readonly Mock<IBackgroundOperation> MockBatteryBlocksOnDockingOperation;

            public readonly Mock<IBackgroundOperation> MockBatteryBlocksOnUndockingOperation;

            public readonly Mock<IBackgroundOperation> MockConnectorsOnDockingOperation;

            public readonly Mock<IBackgroundOperation> MockConnectorsOnUndockingOperation;

            public readonly Mock<IBackgroundOperation> MockFunctionalBlocksOnDockingOperation;

            public readonly Mock<IBackgroundOperation> MockFunctionalBlocksOnUndockingOperation;

            public readonly Mock<IBackgroundOperation> MockGasTanksOnDockingOperation;

            public readonly Mock<IBackgroundOperation> MockGasTanksOnUndockingOperation;

            public readonly Mock<IBackgroundOperation> MockLandingGearsOnDockingOperation;

            public readonly Mock<IBackgroundOperation> MockLandingGearsOnUndockingOperation;

            public void AddMockConnector(MyShipConnectorStatus status)
            {
                var mockConnector = new Mock<IMyShipConnector>();

                mockConnector
                    .Setup(x => x.Status)
                    .Returns(status);

                _mockConnectors.Add(mockConnector);
                _connectors.Add(mockConnector.Object);
            }

            private Mock<IBackgroundOperation> MakeFakeBackgroundOperation()
            {
                var mockBackgroundOperation = new Mock<IBackgroundOperation>();

                mockBackgroundOperation
                    .Setup(x => x.Execute(It.IsAny<Action<IBackgroundOperation>>()))
                    .Returns(BackgroundOperationResult.Completed);

                return mockBackgroundOperation;
            }
        }

        #endregion Test Context

        #region MakeDockOperation() Tests

        [Test]
        public void MakeDockOperation_ConnectorsIsEmpty_LogsError()
        {
            var testContext = new TestContext();

            testContext.Uut.MakeDockOperation()
                .ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.ToLower().Contains("failed") && y.ToLower().Contains("connectors"))));

            testContext.MockBackgroundWorker
                .MockSubOperationScheduler
                .ShouldNotHaveReceived(x => x(It.IsAny<IBackgroundOperation>()));

            testContext.MockBatteryBlockManager
                .Invocations.ShouldBeEmpty();

            testContext.MockConnectorManager
                .ShouldNotHaveReceived(x => x.MakeOnDockingOperation());
            testContext.MockConnectorManager
                .ShouldNotHaveReceived(x => x.MakeOnUndockingOperation());

            testContext.MockFunctionalBlockManager
                .Invocations.ShouldBeEmpty();

            testContext.MockGasTankManager
                .Invocations.ShouldBeEmpty();

            testContext.MockLandingGearManager
                .Invocations.ShouldBeEmpty();
        }

        [TestCase(MyShipConnectorStatus.Connected                                                                        )]
        [TestCase(MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Connected                                     )]
        [TestCase(MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Connected                                     )]
        [TestCase(MyShipConnectorStatus.Connected,   MyShipConnectorStatus.Connectable                                   )]
        [TestCase(MyShipConnectorStatus.Connected,   MyShipConnectorStatus.Unconnected                                   )]
        [TestCase(MyShipConnectorStatus.Connected,   MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Unconnected)]
        [TestCase(MyShipConnectorStatus.Connected,   MyShipConnectorStatus.Connected,   MyShipConnectorStatus.Connected  )]
        public void MakeDockOperation_AnyConnectorIsConnected_LogsError(params MyShipConnectorStatus[] connectorStatuses)
        {
            var testContext = new TestContext();

            foreach (var connectorStatus in connectorStatuses)
                testContext.AddMockConnector(connectorStatus);

            testContext.Uut.MakeDockOperation()
                .ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.ToLower().Contains("failed") && y.ToLower().Contains("docked"))));

            testContext.MockBackgroundWorker
                .MockSubOperationScheduler
                .ShouldNotHaveReceived(x => x(It.IsAny<IBackgroundOperation>()));

            testContext.MockBatteryBlockManager
                .Invocations.ShouldBeEmpty();

            testContext.MockConnectorManager
                .ShouldNotHaveReceived(x => x.MakeOnDockingOperation());
            testContext.MockConnectorManager
                .ShouldNotHaveReceived(x => x.MakeOnUndockingOperation());

            testContext.MockFunctionalBlockManager
                .Invocations.ShouldBeEmpty();

            testContext.MockGasTankManager
                .Invocations.ShouldBeEmpty();

            testContext.MockLandingGearManager
                .Invocations.ShouldBeEmpty();
        }

        [TestCase(MyShipConnectorStatus.Unconnected                                                                      )]
        [TestCase(MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Unconnected                                   )]
        [TestCase(MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Unconnected)]
        public void MakeDockOperation_NoConnectorIsConnectable_LogsError(params MyShipConnectorStatus[] connectorStatuses)
        {
            var testContext = new TestContext();

            foreach (var connectorStatus in connectorStatuses)
                testContext.AddMockConnector(connectorStatus);

            testContext.Uut.MakeDockOperation()
                .ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.ToLower().Contains("failed") && y.ToLower().Contains("connectors") && y.ToLower().Contains("range"))));

            testContext.MockBackgroundWorker
                .MockSubOperationScheduler
                .ShouldNotHaveReceived(x => x(It.IsAny<IBackgroundOperation>()));

            testContext.MockBatteryBlockManager
                .Invocations.ShouldBeEmpty();

            testContext.MockConnectorManager
                .ShouldNotHaveReceived(x => x.MakeOnDockingOperation());
            testContext.MockConnectorManager
                .ShouldNotHaveReceived(x => x.MakeOnUndockingOperation());

            testContext.MockFunctionalBlockManager
                .Invocations.ShouldBeEmpty();

            testContext.MockGasTankManager
                .Invocations.ShouldBeEmpty();

            testContext.MockLandingGearManager
                .Invocations.ShouldBeEmpty();
        }

        [TestCase(MyShipConnectorStatus.Connectable                                                                      )]
        [TestCase(MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Unconnected                                   )]
        [TestCase(MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Connectable                                   )]
        [TestCase(MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Unconnected)]
        [TestCase(MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Unconnected)]
        [TestCase(MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Connectable)]
        [TestCase(MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Connectable)]
        [TestCase(MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Connectable)]
        [TestCase(MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Unconnected)]
        [TestCase(MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Connectable)]
        public void MakeDockOperation_Otherwise_SchedulesDockingOperations(params MyShipConnectorStatus[] connectorStatuses)
        {
            var testContext = new TestContext();

            foreach (var connectorStatus in connectorStatuses)
                testContext.AddMockConnector(connectorStatus);

            testContext.Uut.MakeDockOperation()
                .ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 7);

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.ToLower().Contains("complete"))));

            testContext.MockBatteryBlockManager
                .ShouldHaveReceived(x => x.MakeOnDockingOperation(), 1);
            testContext.MockBatteryBlockManager
                .ShouldNotHaveReceived(x => x.MakeOnUndockingOperation());

            testContext.MockConnectorManager
                .ShouldHaveReceived(x => x.MakeOnDockingOperation(), 1);
            testContext.MockConnectorManager
                .ShouldNotHaveReceived(x => x.MakeOnUndockingOperation());

            testContext.MockFunctionalBlockManager
                .ShouldHaveReceived(x => x.MakeOnDockingOperation(), 1);
            testContext.MockFunctionalBlockManager
                .ShouldNotHaveReceived(x => x.MakeOnUndockingOperation());

            testContext.MockGasTankManager
                .ShouldHaveReceived(x => x.MakeOnDockingOperation(), 1);
            testContext.MockGasTankManager
                .ShouldNotHaveReceived(x => x.MakeOnUndockingOperation());

            testContext.MockLandingGearManager
                .ShouldHaveReceived(x => x.MakeOnDockingOperation(), 1);
            testContext.MockLandingGearManager
                .ShouldNotHaveReceived(x => x.MakeOnUndockingOperation());

            testContext.MockBackgroundWorker
                .MockSubOperationScheduler
                .ShouldHaveReceived(x => x(It.IsAny<IBackgroundOperation>()), 5);
            testContext.MockBackgroundWorker
                .MockSubOperationScheduler
                .Invocations
                .Where(x => x.Method.Name == nameof(Action.Invoke))
                .Select(x => x.Arguments[0])
                .ShouldBe(Enumerable.Empty<object>()
                    .Append(testContext.MockConnectorsOnDockingOperation.Object)
                    .Append(testContext.MockLandingGearsOnDockingOperation.Object)
                    .Append(testContext.MockBatteryBlocksOnDockingOperation.Object)
                    .Append(testContext.MockGasTanksOnDockingOperation.Object)
                    .Append(testContext.MockFunctionalBlocksOnUndockingOperation.Object));
        }

        #endregion MakeDockOperation() Tests

        #region MakeUndockOperation() Tests

        [Test]
        public void MakeUndockOperation_ConnectorsIsEmpty_LogsError()
        {
            var testContext = new TestContext();

            testContext.Uut.MakeUndockOperation()
                .ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.ToLower().Contains("failed") && y.ToLower().Contains("connectors"))));

            testContext.MockBackgroundWorker
                .MockSubOperationScheduler
                .ShouldNotHaveReceived(x => x(It.IsAny<IBackgroundOperation>()));

            testContext.MockBatteryBlockManager
                .Invocations.ShouldBeEmpty();

            testContext.MockConnectorManager
                .ShouldNotHaveReceived(x => x.MakeOnDockingOperation());
            testContext.MockConnectorManager
                .ShouldNotHaveReceived(x => x.MakeOnUndockingOperation());

            testContext.MockFunctionalBlockManager
                .Invocations.ShouldBeEmpty();

            testContext.MockGasTankManager
                .Invocations.ShouldBeEmpty();

            testContext.MockLandingGearManager
                .Invocations.ShouldBeEmpty();
        }

        [TestCase(MyShipConnectorStatus.Connectable                                                                      )]
        [TestCase(MyShipConnectorStatus.Unconnected                                                                      )]
        [TestCase(MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Unconnected                                   )]
        [TestCase(MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Connectable                                   )]
        [TestCase(MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Connectable                                   )]
        [TestCase(MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Unconnected                                   )]
        [TestCase(MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Unconnected)]
        [TestCase(MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Unconnected)]
        [TestCase(MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Connectable)]
        [TestCase(MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Connectable)]
        [TestCase(MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Connectable)]
        [TestCase(MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Unconnected)]
        [TestCase(MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Connectable)]
        [TestCase(MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Unconnected)]
        public void MakeUndockOperation_NoConnectorIsConnected_LogsError(params MyShipConnectorStatus[] connectorStatuses)
        {
            var testContext = new TestContext();

            foreach (var connectorStatus in connectorStatuses)
                testContext.AddMockConnector(connectorStatus);

            testContext.Uut.MakeUndockOperation()
                .ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.ToLower().Contains("failed") && y.ToLower().Contains("docked"))));

            testContext.MockBackgroundWorker
                .MockSubOperationScheduler
                .ShouldNotHaveReceived(x => x(It.IsAny<IBackgroundOperation>()));

            testContext.MockBatteryBlockManager
                .Invocations.ShouldBeEmpty();

            testContext.MockConnectorManager
                .ShouldNotHaveReceived(x => x.MakeOnDockingOperation());
            testContext.MockConnectorManager
                .ShouldNotHaveReceived(x => x.MakeOnUndockingOperation());

            testContext.MockFunctionalBlockManager
                .Invocations.ShouldBeEmpty();

            testContext.MockGasTankManager
                .Invocations.ShouldBeEmpty();

            testContext.MockLandingGearManager
                .Invocations.ShouldBeEmpty();
        }

        [TestCase(MyShipConnectorStatus.Connected                                                                        )]
        [TestCase(MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Connected                                     )]
        [TestCase(MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Connected                                     )]
        [TestCase(MyShipConnectorStatus.Connected,   MyShipConnectorStatus.Connectable                                   )]
        [TestCase(MyShipConnectorStatus.Connected,   MyShipConnectorStatus.Unconnected                                   )]
        [TestCase(MyShipConnectorStatus.Connected,   MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Unconnected)]
        [TestCase(MyShipConnectorStatus.Connected,   MyShipConnectorStatus.Connected,   MyShipConnectorStatus.Connected  )]
        public void MakeUndockOperation_Otherwise_SchedulesUndockingOperations(params MyShipConnectorStatus[] connectorStatuses)
        {
            var testContext = new TestContext();

            foreach (var connectorStatus in connectorStatuses)
                testContext.AddMockConnector(connectorStatus);

            testContext.Uut.MakeUndockOperation()
                .ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 7);

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.ToLower().Contains("complete"))));

            testContext.MockBatteryBlockManager
                .ShouldHaveReceived(x => x.MakeOnUndockingOperation(), 1);
            testContext.MockBatteryBlockManager
                .ShouldNotHaveReceived(x => x.MakeOnDockingOperation());

            testContext.MockConnectorManager
                .ShouldHaveReceived(x => x.MakeOnUndockingOperation(), 1);
            testContext.MockConnectorManager
                .ShouldNotHaveReceived(x => x.MakeOnDockingOperation());

            testContext.MockFunctionalBlockManager
                .ShouldHaveReceived(x => x.MakeOnUndockingOperation(), 1);
            testContext.MockFunctionalBlockManager
                .ShouldNotHaveReceived(x => x.MakeOnDockingOperation());

            testContext.MockGasTankManager
                .ShouldHaveReceived(x => x.MakeOnUndockingOperation(), 1);
            testContext.MockGasTankManager
                .ShouldNotHaveReceived(x => x.MakeOnDockingOperation());

            testContext.MockLandingGearManager
                .ShouldHaveReceived(x => x.MakeOnUndockingOperation(), 1);
            testContext.MockLandingGearManager
                .ShouldNotHaveReceived(x => x.MakeOnDockingOperation());

            testContext.MockBackgroundWorker
                .MockSubOperationScheduler
                .ShouldHaveReceived(x => x(It.IsAny<IBackgroundOperation>()), 5);
            testContext.MockBackgroundWorker
                .MockSubOperationScheduler
                .Invocations
                .Where(x => x.Method.Name == nameof(Action.Invoke))
                .Select(x => x.Arguments[0])
                .ShouldBe(Enumerable.Empty<object>()
                    .Append(testContext.MockFunctionalBlocksOnDockingOperation.Object)
                    .Append(testContext.MockGasTanksOnUndockingOperation.Object)
                    .Append(testContext.MockBatteryBlocksOnUndockingOperation.Object)
                    .Append(testContext.MockLandingGearsOnUndockingOperation.Object)
                    .Append(testContext.MockConnectorsOnUndockingOperation.Object));
        }

        #endregion MakeUndockOperation() Tests

        #region MakeToggleOperation() Tests

        [Test]
        public void MakeToggleOperation_ConnectorsIsEmpty_LogsError()
        {
            var testContext = new TestContext();

            testContext.Uut.MakeToggleOperation()
                .ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.ToLower().Contains("failed") && y.ToLower().Contains("connectors"))));

            testContext.MockBackgroundWorker
                .MockSubOperationScheduler
                .ShouldNotHaveReceived(x => x(It.IsAny<IBackgroundOperation>()));

            testContext.MockBatteryBlockManager
                .Invocations.ShouldBeEmpty();

            testContext.MockConnectorManager
                .ShouldNotHaveReceived(x => x.MakeOnDockingOperation());
            testContext.MockConnectorManager
                .ShouldNotHaveReceived(x => x.MakeOnUndockingOperation());

            testContext.MockFunctionalBlockManager
                .Invocations.ShouldBeEmpty();

            testContext.MockGasTankManager
                .Invocations.ShouldBeEmpty();

            testContext.MockLandingGearManager
                .Invocations.ShouldBeEmpty();
        }

        [TestCase(MyShipConnectorStatus.Connected)]
        [TestCase(MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Connected)]
        [TestCase(MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Connected)]
        [TestCase(MyShipConnectorStatus.Connected, MyShipConnectorStatus.Connectable)]
        [TestCase(MyShipConnectorStatus.Connected, MyShipConnectorStatus.Unconnected)]
        [TestCase(MyShipConnectorStatus.Connected, MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Unconnected)]
        [TestCase(MyShipConnectorStatus.Connected, MyShipConnectorStatus.Connected, MyShipConnectorStatus.Connected)]
        public void MakeToggleOperation_AnyConnectorIsConnected_SchedulesUndockingOperations(params MyShipConnectorStatus[] connectorStatuses)
        {
            var testContext = new TestContext();

            foreach (var connectorStatus in connectorStatuses)
                testContext.AddMockConnector(connectorStatus);

            testContext.Uut.MakeToggleOperation()
                .ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 7);

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.ToLower().Contains("complete"))));

            testContext.MockBatteryBlockManager
                .ShouldHaveReceived(x => x.MakeOnUndockingOperation(), 1);
            testContext.MockBatteryBlockManager
                .ShouldNotHaveReceived(x => x.MakeOnDockingOperation());

            testContext.MockConnectorManager
                .ShouldHaveReceived(x => x.MakeOnUndockingOperation(), 1);
            testContext.MockConnectorManager
                .ShouldNotHaveReceived(x => x.MakeOnDockingOperation());

            testContext.MockFunctionalBlockManager
                .ShouldHaveReceived(x => x.MakeOnUndockingOperation(), 1);
            testContext.MockFunctionalBlockManager
                .ShouldNotHaveReceived(x => x.MakeOnDockingOperation());

            testContext.MockGasTankManager
                .ShouldHaveReceived(x => x.MakeOnUndockingOperation(), 1);
            testContext.MockGasTankManager
                .ShouldNotHaveReceived(x => x.MakeOnDockingOperation());

            testContext.MockLandingGearManager
                .ShouldHaveReceived(x => x.MakeOnUndockingOperation(), 1);
            testContext.MockLandingGearManager
                .ShouldNotHaveReceived(x => x.MakeOnDockingOperation());

            testContext.MockBackgroundWorker
                .MockSubOperationScheduler
                .ShouldHaveReceived(x => x(It.IsAny<IBackgroundOperation>()), 5);
            testContext.MockBackgroundWorker
                .MockSubOperationScheduler
                .Invocations
                .Where(x => x.Method.Name == nameof(Action.Invoke))
                .Select(x => x.Arguments[0])
                .ShouldBe(Enumerable.Empty<object>()
                    .Append(testContext.MockFunctionalBlocksOnDockingOperation.Object)
                    .Append(testContext.MockGasTanksOnUndockingOperation.Object)
                    .Append(testContext.MockBatteryBlocksOnUndockingOperation.Object)
                    .Append(testContext.MockLandingGearsOnUndockingOperation.Object)
                    .Append(testContext.MockConnectorsOnUndockingOperation.Object));
        }

        [TestCase(MyShipConnectorStatus.Connectable)]
        [TestCase(MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Unconnected)]
        [TestCase(MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Connectable)]
        [TestCase(MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Connectable)]
        [TestCase(MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Unconnected)]
        [TestCase(MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Unconnected)]
        [TestCase(MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Connectable)]
        [TestCase(MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Connectable)]
        [TestCase(MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Connectable)]
        [TestCase(MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Unconnected)]
        [TestCase(MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Connectable, MyShipConnectorStatus.Connectable)]
        public void MakeToggleOperation_NoConnectorIsConnectedAndAnyConnectorIsConnectable_SchedulesDockingOperations(params MyShipConnectorStatus[] connectorStatuses)
        {
            var testContext = new TestContext();

            foreach (var connectorStatus in connectorStatuses)
                testContext.AddMockConnector(connectorStatus);

            testContext.Uut.MakeToggleOperation()
                .ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 7);

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.ToLower().Contains("complete"))));

            testContext.MockBatteryBlockManager
                .ShouldHaveReceived(x => x.MakeOnDockingOperation(), 1);
            testContext.MockBatteryBlockManager
                .ShouldNotHaveReceived(x => x.MakeOnUndockingOperation());

            testContext.MockConnectorManager
                .ShouldHaveReceived(x => x.MakeOnDockingOperation(), 1);
            testContext.MockConnectorManager
                .ShouldNotHaveReceived(x => x.MakeOnUndockingOperation());

            testContext.MockFunctionalBlockManager
                .ShouldHaveReceived(x => x.MakeOnDockingOperation(), 1);
            testContext.MockFunctionalBlockManager
                .ShouldNotHaveReceived(x => x.MakeOnUndockingOperation());

            testContext.MockGasTankManager
                .ShouldHaveReceived(x => x.MakeOnDockingOperation(), 1);
            testContext.MockGasTankManager
                .ShouldNotHaveReceived(x => x.MakeOnUndockingOperation());

            testContext.MockLandingGearManager
                .ShouldHaveReceived(x => x.MakeOnDockingOperation(), 1);
            testContext.MockLandingGearManager
                .ShouldNotHaveReceived(x => x.MakeOnUndockingOperation());

            testContext.MockBackgroundWorker
                .MockSubOperationScheduler
                .ShouldHaveReceived(x => x(It.IsAny<IBackgroundOperation>()), 5);
            testContext.MockBackgroundWorker
                .MockSubOperationScheduler
                .Invocations
                .Where(x => x.Method.Name == nameof(Action.Invoke))
                .Select(x => x.Arguments[0])
                .ShouldBe(Enumerable.Empty<object>()
                    .Append(testContext.MockConnectorsOnDockingOperation.Object)
                    .Append(testContext.MockLandingGearsOnDockingOperation.Object)
                    .Append(testContext.MockBatteryBlocksOnDockingOperation.Object)
                    .Append(testContext.MockGasTanksOnDockingOperation.Object)
                    .Append(testContext.MockFunctionalBlocksOnUndockingOperation.Object));
        }

        [TestCase(MyShipConnectorStatus.Unconnected)]
        [TestCase(MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Unconnected)]
        [TestCase(MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Unconnected, MyShipConnectorStatus.Unconnected)]
        public void MakeToggleOperation_Otherwise_LogsError(params MyShipConnectorStatus[] connectorStatuses)
        {
            var testContext = new TestContext();

            foreach (var connectorStatus in connectorStatuses)
                testContext.AddMockConnector(connectorStatus);

            testContext.Uut.MakeToggleOperation()
                .ShouldRunToCompletionIn(testContext.MockBackgroundWorker, 1);

            testContext.MockLogger
                .ShouldHaveReceived(x => x.AddLine(It.Is<string>(y => y.ToLower().Contains("failed") && y.ToLower().Contains("docked") && y.ToLower().Contains("dockable"))));

            testContext.MockBackgroundWorker
                .MockSubOperationScheduler
                .ShouldNotHaveReceived(x => x(It.IsAny<IBackgroundOperation>()));

            testContext.MockBatteryBlockManager
                .Invocations.ShouldBeEmpty();

            testContext.MockConnectorManager
                .ShouldNotHaveReceived(x => x.MakeOnDockingOperation());
            testContext.MockConnectorManager
                .ShouldNotHaveReceived(x => x.MakeOnUndockingOperation());

            testContext.MockFunctionalBlockManager
                .Invocations.ShouldBeEmpty();

            testContext.MockGasTankManager
                .Invocations.ShouldBeEmpty();

            testContext.MockLandingGearManager
                .Invocations.ShouldBeEmpty();
        }

        #endregion MakeUndockOperation() Tests
    }
}
