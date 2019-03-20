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
                    .Setup(x => x.MakeDischargeOperation())
                    .Returns(() => MockDischargeBatteryBlocksOperation.Object);
                MockBatteryBlockManager
                    .Setup(x => x.MakeRechargeOperation())
                    .Returns(() => MockRechargeBatteryBlocksOperation.Object);

                MockConnectorManager = new Mock<IConnectorManager>();
                MockConnectorManager
                    .Setup(x => x.Connectors)
                    .Returns(() => _connectors);
                MockConnectorManager
                    .Setup(x => x.MakeConnectOperation())
                    .Returns(() => MockConnectConnectorsOperation.Object);
                MockConnectorManager
                    .Setup(x => x.MakeDisconnectOperation())
                    .Returns(() => MockDisconnectConnectorsOperation.Object);

                MockFunctionalBlockManager = new Mock<IFunctionalBlockManager>();
                MockFunctionalBlockManager
                    .Setup(x => x.MakeEnableOperation())
                    .Returns(() => MockEnableFunctionalBlocksOperation.Object);
                MockFunctionalBlockManager
                    .Setup(x => x.MakeDisableOperation())
                    .Returns(() => MockDisableFunctionalBlocksOperation.Object);

                MockGasTankManager = new Mock<IGasTankManager>();
                MockGasTankManager
                    .Setup(x => x.MakeStockpileOperation())
                    .Returns(() => MockStockpileGasTanksOperation.Object);
                MockGasTankManager
                    .Setup(x => x.MakeDispenseOperation())
                    .Returns(() => MockDispenseGasTanksOperation.Object);

                MockLandingGearManager = new Mock<ILandingGearManager>();
                MockLandingGearManager
                    .Setup(x => x.MakeLockOperation())
                    .Returns(() => MockLockLandingGearsOperation.Object);
                MockLandingGearManager
                    .Setup(x => x.MakeUnlockOperation())
                    .Returns(() => MockUnlockLandingGearsOperation.Object);

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

                MockRechargeBatteryBlocksOperation = MakeFakeBackgroundOperation();

                MockDischargeBatteryBlocksOperation = MakeFakeBackgroundOperation();

                MockConnectConnectorsOperation = MakeFakeBackgroundOperation();

                MockDisconnectConnectorsOperation = MakeFakeBackgroundOperation();

                MockEnableFunctionalBlocksOperation = MakeFakeBackgroundOperation();

                MockDisableFunctionalBlocksOperation = MakeFakeBackgroundOperation();

                MockStockpileGasTanksOperation = MakeFakeBackgroundOperation();

                MockDispenseGasTanksOperation = MakeFakeBackgroundOperation();

                MockLockLandingGearsOperation = MakeFakeBackgroundOperation();

                MockUnlockLandingGearsOperation = MakeFakeBackgroundOperation();
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

            public readonly Mock<IBackgroundOperation> MockRechargeBatteryBlocksOperation;

            public readonly Mock<IBackgroundOperation> MockDischargeBatteryBlocksOperation;

            public readonly Mock<IBackgroundOperation> MockConnectConnectorsOperation;

            public readonly Mock<IBackgroundOperation> MockDisconnectConnectorsOperation;

            public readonly Mock<IBackgroundOperation> MockEnableFunctionalBlocksOperation;

            public readonly Mock<IBackgroundOperation> MockDisableFunctionalBlocksOperation;

            public readonly Mock<IBackgroundOperation> MockStockpileGasTanksOperation;

            public readonly Mock<IBackgroundOperation> MockDispenseGasTanksOperation;

            public readonly Mock<IBackgroundOperation> MockLockLandingGearsOperation;

            public readonly Mock<IBackgroundOperation> MockUnlockLandingGearsOperation;

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
                .ShouldNotHaveReceived(x => x.MakeConnectOperation());
            testContext.MockConnectorManager
                .ShouldNotHaveReceived(x => x.MakeDisconnectOperation());

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
                .ShouldNotHaveReceived(x => x.MakeConnectOperation());
            testContext.MockConnectorManager
                .ShouldNotHaveReceived(x => x.MakeDisconnectOperation());

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
                .ShouldNotHaveReceived(x => x.MakeConnectOperation());
            testContext.MockConnectorManager
                .ShouldNotHaveReceived(x => x.MakeDisconnectOperation());

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
                .ShouldHaveReceived(x => x.MakeRechargeOperation(), 1);
            testContext.MockBatteryBlockManager
                .ShouldNotHaveReceived(x => x.MakeDischargeOperation());

            testContext.MockConnectorManager
                .ShouldHaveReceived(x => x.MakeConnectOperation(), 1);
            testContext.MockConnectorManager
                .ShouldNotHaveReceived(x => x.MakeDisconnectOperation());

            testContext.MockFunctionalBlockManager
                .ShouldHaveReceived(x => x.MakeDisableOperation(), 1);
            testContext.MockFunctionalBlockManager
                .ShouldNotHaveReceived(x => x.MakeEnableOperation());

            testContext.MockGasTankManager
                .ShouldHaveReceived(x => x.MakeStockpileOperation(), 1);
            testContext.MockGasTankManager
                .ShouldNotHaveReceived(x => x.MakeDispenseOperation());

            testContext.MockLandingGearManager
                .ShouldHaveReceived(x => x.MakeLockOperation(), 1);
            testContext.MockLandingGearManager
                .ShouldNotHaveReceived(x => x.MakeUnlockOperation());

            testContext.MockBackgroundWorker
                .MockSubOperationScheduler
                .ShouldHaveReceived(x => x(It.IsAny<IBackgroundOperation>()), 5);
            testContext.MockBackgroundWorker
                .MockSubOperationScheduler
                .Invocations
                .Where(x => x.Method.Name == nameof(Action.Invoke))
                .Select(x => x.Arguments[0])
                .ShouldBe(Enumerable.Empty<object>()
                    .Append(testContext.MockConnectConnectorsOperation.Object)
                    .Append(testContext.MockLockLandingGearsOperation.Object)
                    .Append(testContext.MockRechargeBatteryBlocksOperation.Object)
                    .Append(testContext.MockStockpileGasTanksOperation.Object)
                    .Append(testContext.MockDisableFunctionalBlocksOperation.Object));
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
                .ShouldNotHaveReceived(x => x.MakeConnectOperation());
            testContext.MockConnectorManager
                .ShouldNotHaveReceived(x => x.MakeDisconnectOperation());

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
                .ShouldNotHaveReceived(x => x.MakeConnectOperation());
            testContext.MockConnectorManager
                .ShouldNotHaveReceived(x => x.MakeDisconnectOperation());

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
                .ShouldHaveReceived(x => x.MakeDischargeOperation(), 1);
            testContext.MockBatteryBlockManager
                .ShouldNotHaveReceived(x => x.MakeRechargeOperation());

            testContext.MockConnectorManager
                .ShouldHaveReceived(x => x.MakeDisconnectOperation(), 1);
            testContext.MockConnectorManager
                .ShouldNotHaveReceived(x => x.MakeConnectOperation());

            testContext.MockFunctionalBlockManager
                .ShouldHaveReceived(x => x.MakeEnableOperation(), 1);
            testContext.MockFunctionalBlockManager
                .ShouldNotHaveReceived(x => x.MakeDisableOperation());

            testContext.MockGasTankManager
                .ShouldHaveReceived(x => x.MakeDispenseOperation(), 1);
            testContext.MockGasTankManager
                .ShouldNotHaveReceived(x => x.MakeStockpileOperation());

            testContext.MockLandingGearManager
                .ShouldHaveReceived(x => x.MakeUnlockOperation(), 1);
            testContext.MockLandingGearManager
                .ShouldNotHaveReceived(x => x.MakeLockOperation());

            testContext.MockBackgroundWorker
                .MockSubOperationScheduler
                .ShouldHaveReceived(x => x(It.IsAny<IBackgroundOperation>()), 5);
            testContext.MockBackgroundWorker
                .MockSubOperationScheduler
                .Invocations
                .Where(x => x.Method.Name == nameof(Action.Invoke))
                .Select(x => x.Arguments[0])
                .ShouldBe(Enumerable.Empty<object>()
                    .Append(testContext.MockEnableFunctionalBlocksOperation.Object)
                    .Append(testContext.MockDispenseGasTanksOperation.Object)
                    .Append(testContext.MockDischargeBatteryBlocksOperation.Object)
                    .Append(testContext.MockUnlockLandingGearsOperation.Object)
                    .Append(testContext.MockDisconnectConnectorsOperation.Object));
        }

        #endregion MakeUndockOperation() Tests
    }
}
