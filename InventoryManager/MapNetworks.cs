//using Sandbox.Game.EntityComponents;
//using Sandbox.ModAPI.Ingame;
//using Sandbox.ModAPI.Interfaces;
//using SpaceEngineers.Game.ModAPI.Ingame;
//using System.Collections.Generic;
//using System.Collections;
//using System.Linq;
//using System.Text;
//using System;
//using VRage.Collections;
//using VRage.Game.Components;
//using VRage.Game.ModAPI.Ingame;
//using VRage.Game.ModAPI.Ingame.Utilities;
//using VRage.Game.ObjectBuilders.Definitions;
//using VRage.Game;
//using VRageMath;

//namespace IngameScript
//{
//    partial class Program
//    {
//        private void AutoRemapNetworks()
//        {
//            if (_scriptConfig.AutoRemapGrids)
//            {
//                var connectors = _connectorHashSetObjectPool.Get();

//                GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(null, connector =>
//                {
//                    connectors.Add(connector);
//                    return false;
//                });

//                if (!_connectors.SetEquals(connectors))
//                {
//                    _connectors.Clear();
//                    _connectorHashSetObjectPool.Put(_connectors);
//                    _connectors = connectors;
//                    MapNetworks();
//                }
//            }
//        }

//        private void MapNetworks()
//        {
//            foreach(var network in _networks)
//            {
//                network.Clear();
//                _networkObjectPool.Put(network);
//            }
//            _networks.Clear();

//            GridTerminalSystem.GetBlocksOfType(null as List<IMyTerminalBlock>, CollectTerminalBlock);
//        }

//        private bool CollectTerminalBlock(IMyTerminalBlock block)
//        {
//            if (!block.HasInventory)
//                return false;

//            if (!_scriptConfig.ManageOtherGrids && (block.CubeGrid.EntityId != Me.CubeGrid.EntityId))
//                return false;

//            var blockConfig = ParseBlockConfig(block);

//            if (blockConfig.Ignore || (blockConfig.IgnoreInput && blockConfig.IgnoreOutput))
//                return false;

//            if (!blockConfig.Manage && !blockConfig.Requests.Any() && !((block.CubeGrid.EntityId == Me.CubeGrid.EntityId) ? _scriptConfig.AutoManageThisGrid : _scriptConfig.AutoManageOtherGrids))
//                return false;

//            if (block.InventoryCount == 1)
//            {
//                AddToNetworks(block.GetInventory(0), blockConfig.Requests);
//            }
//            else if (block.InventoryCount == 2)
//            {
//                if (!_scriptConfig.IgnoreProductionInputs && !blockConfig.IgnoreInput)
//                    AddToNetworks(block.GetInventory(0), blockConfig.Requests);
//                if (!blockConfig.IgnoreOutput)
//                    AddToNetworks(block.GetInventory(1), _itemRequestListObjectPool.Get());
//            }

//            return false;
//        }

//        private void AddToNetworks(IMyInventory inventory, List<ItemRequest> requests)
//        {
//            foreach (var network in _networks)
//                if (network.TryAddInventory(inventory, requests))
//                    return;

//            var newNetwork = _networkObjectPool.Get();
//            newNetwork.TryAddInventory(inventory, requests);
//            _networks.Add(newNetwork);
//        }
//    }
//}
