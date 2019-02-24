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
//    partial class Program : MyGridProgram
//    {
//        public Program()
//        {
//            _networkObjectPool = new ObjectPool<ItemNetwork>(() => new ItemNetwork(this));

//            _operations.Enqueue(ReloadConfig().GetEnumerator());

//            Runtime.UpdateFrequency = UpdateFrequency.Once;
//        }

//        public void Main(string argument)
//        {
//            var now = DateTime.Now;
//            Echo($"{now}");

//            switch (argument)
//            {
//                case "":
//                    _transferCount = 0;
//                    Run();
//                    if (_transferCount > 0)
//                        Log($"{now.ToString("hh:mm:ss")}: Performed {_transferCount} transfer{((_transferCount == 1) ? string.Empty : "s")}");
//                    break;

//                case "reload-config":
//                    _operations.Enqueue(ReloadConfig().GetEnumerator());
//                    break;

//                case "remap-grids":
//                    _operations.Enqueue(RemapGrids().GetEnumerator());
//                    break;

//                default:
//                    Log($"Invalid argument \"{argument}\"");
//                    break;
//            }

//            _timeSinceLastProcess += Runtime.TimeSinceLastRun;
//            if (_timeSinceLastProcess > _scriptConfig.ProcessInterval)
//                _operations.Enqueue(ProcessRequests().GetEnumerator());

//            _timeSinceLastClean += Runtime.TimeSinceLastRun;
//            if (_timeSinceLastClean > _scriptConfig.CleanInterval)
//                _operations.Enqueue(Clean().GetEnumerator());

//            Runtime.UpdateFrequency = _operations.Any()
//                ? UpdateFrequency.Once
//                : UpdateFrequency.Update10;

//            Echo(string.Concat(_log.Reverse()));
//        }

//        private void LoadConfig()
//        {
//            _scriptConfig = ParseScriptConfig();
//        }

//        private void Run()
//        {
//            while(_operations.Any())
//            {
//                var operation = _operations.Peek();

//                while (operation.MoveNext())
//                    if (Runtime.CurrentInstructionCount >= _scriptConfig.InstructionsPerCycle)
//                        return;

//                _operations.Dequeue();
//            }
//        }

//        private IEnumerable<object> ReloadConfig()
//        {
//            LoadConfig();
//            yield return null;
//            MapNetworks();
//            yield return null;
//        }

//        private IEnumerable<object> RemapGrids()
//        {
//            MapNetworks();
//            yield return null;
//        }

//        private IEnumerable<object> ProcessRequests()
//        {
//            AutoRemapNetworks();
//            yield return null;

//            foreach (var network in _networks)
//                foreach (var result in network.ProcessRequests())
//                    yield return result;

//            _timeSinceLastProcess = TimeSpan.Zero;
//        }

//        private IEnumerable<object> Clean()
//        {
//            _connectorHashSetObjectPool.PruneUnused();
//            yield return null;
//            _inventoryAllocationDictionaryObjectPool.PruneUnused();
//            yield return null;
//            _inventoryRequestListObjectPool.PruneUnused();
//            yield return null;
//            _itemRequestListObjectPool.PruneUnused();
//            yield return null;
//            _networkObjectPool.PruneUnused();
//            yield return null;

//            _connectorHashSetObjectPool.TrimCapacity();
//            yield return null;
//            _inventoryAllocationDictionaryObjectPool.TrimCapacity();
//            yield return null;
//            _inventoryRequestListObjectPool.TrimCapacity();
//            yield return null;
//            _itemRequestListObjectPool.TrimCapacity();
//            yield return null;
//            _networkObjectPool.TrimCapacity();
//            yield return null;

//            _timeSinceLastClean = TimeSpan.Zero;
//        }

//        private void Log(string line)
//        {
//            _log.Enqueue('\n' + line);
//            if (_log.Count > _scriptConfig.LogSize)
//                _log.Dequeue();
//        }

//        private ScriptConfig _scriptConfig;

//        private TimeSpan _timeSinceLastProcess
//            = TimeSpan.Zero;

//        private TimeSpan _timeSinceLastClean
//            = TimeSpan.Zero;

//        private int _transferCount;

//        private readonly Queue<string> _log
//            = new Queue<string>();

//        private readonly Queue<IEnumerator<object>> _operations
//            = new Queue<IEnumerator<object>>();

//        private HashSet<IMyShipConnector> _connectors
//            = new HashSet<IMyShipConnector>();

//        private readonly List<ItemNetwork> _networks
//            = new List<ItemNetwork>();

//        private readonly ObjectPool<ItemNetwork> _networkObjectPool;

//        private readonly ObjectPool<HashSet<IMyShipConnector>> _connectorHashSetObjectPool
//            = new ObjectPool<HashSet<IMyShipConnector>>(() => new HashSet<IMyShipConnector>());

//        private readonly ObjectPool<List<ItemRequest>> _itemRequestListObjectPool
//            = new ObjectPool<List<ItemRequest>>(() => new List<ItemRequest>());

//        private readonly ObjectPool<List<InventoryRequest>> _inventoryRequestListObjectPool
//            = new ObjectPool<List<InventoryRequest>>(() => new List<InventoryRequest>());

//        private readonly ObjectPool<Dictionary<IMyInventory, Allocation>> _inventoryAllocationDictionaryObjectPool
//            = new ObjectPool<Dictionary<IMyInventory, Allocation>>(() => new Dictionary<IMyInventory, Allocation>());
//    }
//}