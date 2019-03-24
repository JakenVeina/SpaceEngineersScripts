using System;

using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public interface IConnectorManager : IBlockManager<IMyShipConnector> { }

        public sealed class ConnectorManager : BlockManagerBase<IMyShipConnector>, IConnectorManager
        {
            internal protected override OnDockOperationBase CreateOnDockingOperation(BlockManagerBase<IMyShipConnector> owner, Action onDisposed)
                => new ConnectOperation(owner, onDisposed);

            internal protected override OnDockOperationBase CreateOnUndockingOperation(BlockManagerBase<IMyShipConnector> owner, Action onDisposed)
                => new DisconnectOperation(owner, onDisposed);

            private sealed class ConnectOperation : OnDockOperationBase
            {
                public ConnectOperation(BlockManagerBase<IMyShipConnector> owner, Action onDisposed)
                    : base(owner, onDisposed) { }

                protected override void OnExecuting(IMyShipConnector connector)
                    => connector.Connect();
            }

            private sealed class DisconnectOperation : OnDockOperationBase
            {
                public DisconnectOperation(BlockManagerBase<IMyShipConnector> owner, Action onDisposed)
                    : base(owner, onDisposed) { }

                protected override void OnExecuting(IMyShipConnector connector)
                    => connector.Disconnect();
            }
        }
    }
}
