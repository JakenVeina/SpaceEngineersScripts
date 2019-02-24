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
//        private class ItemNetwork
//        {
//            public ItemNetwork(Program program)
//            {
//                _program = program;
//            }

//            public bool TryAddInventory(IMyInventory inventory, List<ItemRequest> requests)
//            {
//                var existingInventory = Inventories.FirstOrDefault();
//                if ((existingInventory != null) && !existingInventory.IsConnectedTo(inventory))
//                    return false;

//                Inventories.Add(inventory);

//                foreach (var request in requests)
//                {
//                    List<InventoryRequest> inventoryRequests;

//                    if (!InventoryRequestsByItemDefinitionId.TryGetValue(request.ItemDefinitionId, out inventoryRequests))
//                    {
//                        inventoryRequests = _program._inventoryRequestListObjectPool.Get();
//                        InventoryRequestsByItemDefinitionId.Add(request.ItemDefinitionId, inventoryRequests);
//                    }

//                    inventoryRequests.Add(new InventoryRequest()
//                    {
//                        Inventory = inventory,
//                        Amount = request.Amount
//                    });
//                }

//                return true;
//            }

//            public IEnumerable<object> ProcessRequests()
//            {
//                foreach (var result in CalculateTotals())
//                    yield return result;

//                foreach (var result in AllocateItems())
//                    yield return result;

//                foreach (var result in TransferItems())
//                    yield return result;
//            }

//            public void Clear()
//            {
//                Inventories.Clear();

//                foreach (var inventoryRequestList in InventoryRequestsByItemDefinitionId.Values)
//                {
//                    inventoryRequestList.Clear();
//                    _program._inventoryRequestListObjectPool.Put(inventoryRequestList);
//                }
//                InventoryRequestsByItemDefinitionId.Clear();

//                TotalAvailableAmountsByItemDefinitionId.Clear();

//                foreach(var inventoryAllocationDictionary in AllocationsByInventoryByItemDefinitionId.Values)
//                {
//                    inventoryAllocationDictionary.Clear();
//                    _program._inventoryAllocationDictionaryObjectPool.Put(inventoryAllocationDictionary);
//                }
//                AllocationsByInventoryByItemDefinitionId.Clear();
//            }

//            private IEnumerable<object> CalculateTotals()
//            {
//                TotalAvailableAmountsByItemDefinitionId.Clear();

//                foreach (var inventory in Inventories)
//                {
//                    yield return null;

//                    foreach (var item in inventory.GetItems())
//                    {
//                        var itemDefinitionId = item.GetDefinitionId().ToString();

//                        if (!InventoryRequestsByItemDefinitionId.ContainsKey(itemDefinitionId))
//                            continue;

//                        var totalAmount = VRage.MyFixedPoint.Zero;
//                        TotalAvailableAmountsByItemDefinitionId.TryGetValue(itemDefinitionId, out totalAmount);
//                        totalAmount += item.Amount;
//                        TotalAvailableAmountsByItemDefinitionId[itemDefinitionId] = totalAmount;
//                    }
//                }
//            }

//            private IEnumerable<object> AllocateItems()
//            {
//                AllocationsByInventoryByItemDefinitionId.Clear();

//                foreach (var pair in InventoryRequestsByItemDefinitionId)
//                {
//                    yield return null;

//                    var totalRequestedAmount = VRage.MyFixedPoint.Zero;
//                    var nullRequestCount = 0;

//                    foreach (var request in pair.Value)
//                    {
//                        if (request.Amount != null)
//                            totalRequestedAmount += request.Amount.Value;
//                        else
//                            ++nullRequestCount;
//                    }

//                    VRage.MyFixedPoint totalAvailableAmount;
//                    if (!TotalAvailableAmountsByItemDefinitionId.TryGetValue(pair.Key, out totalAvailableAmount))
//                        continue;

//                    var leftoverAmount = totalAvailableAmount - totalRequestedAmount;

//                    VRage.MyFixedPoint requestScale;
//                    VRage.MyFixedPoint? nullRequestAmount = null;
//                    if (leftoverAmount < 0)
//                        requestScale = (VRage.MyFixedPoint)((double)totalRequestedAmount / (double)totalAvailableAmount);
//                    else
//                    {
//                        requestScale = 1;
//                        if (nullRequestCount > 0)
//                            nullRequestAmount = (VRage.MyFixedPoint)((double)leftoverAmount / nullRequestCount);
//                    }

//                    foreach (var request in pair.Value)
//                    {
//                        if (request.Amount != null)
//                            Allocate(pair.Key, request.Inventory, request.Amount.Value * requestScale);
//                        else if (nullRequestAmount != null)
//                            Allocate(pair.Key, request.Inventory, nullRequestAmount.Value);
//                    }
//                }
//            }

//            private IEnumerable<object> TransferItems()
//            {
//                foreach (var sourceInventory in Inventories)
//                {
//                    yield return null;

//                    var items = sourceInventory.GetItems();
//                    for (var i = 0; i < items.Count; ++i)
//                    {
//                        var item = items[i];

//                        var itemDefinitionId = item.GetDefinitionId();
//                        var itemDefinitionIdString = item.GetDefinitionId().ToString();

//                        Dictionary<IMyInventory, Allocation> allocationsByInventory;
//                        if (!AllocationsByInventoryByItemDefinitionId.TryGetValue(itemDefinitionIdString, out allocationsByInventory))
//                            continue;

//                        Allocation selfAllocation;
//                        if(allocationsByInventory.TryGetValue(sourceInventory, out selfAllocation))
//                        {
//                            var currentSelfAmount = sourceInventory.GetItemAmount(itemDefinitionId);
//                            if (currentSelfAmount < selfAllocation.Amount)
//                                continue;
//                            else
//                            {
//                                selfAllocation.IsFulfilled = true;
//                                allocationsByInventory[sourceInventory] = selfAllocation;
//                            }
//                        }

//                        var targetInventories = allocationsByInventory.Keys.ToArray();
//                        foreach (var targetInventory in targetInventories)
//                        {
//                            var allocation = allocationsByInventory[targetInventory];
//                            if (allocationsByInventory[targetInventory].IsFulfilled)
//                                continue;

//                            var existingAmount = targetInventory.GetItemAmount(itemDefinitionId);

//                            var transferAmount = allocation.Amount - existingAmount;
//                            if (transferAmount <= VRage.MyFixedPoint.Zero)
//                            {
//                                allocation.IsFulfilled = true;
//                                allocationsByInventory[targetInventory] = allocation;
//                                continue;
//                            }

//                            var extraSelfAmount = sourceInventory.GetItemAmount(itemDefinitionId) - selfAllocation.Amount;
//                            if (transferAmount > extraSelfAmount)
//                                transferAmount = extraSelfAmount;

//                            targetInventory.TransferItemFrom(sourceInventory, item, transferAmount);

//                            var resultAmount = targetInventory.GetItemAmount(itemDefinitionId);
//                            if (resultAmount != existingAmount)
//                                ++_program._transferCount;

//                            if(resultAmount >= allocation.Amount)
//                            {
//                                allocation.IsFulfilled = true;
//                                allocationsByInventory[targetInventory] = allocation;
//                            }
//                        }
//                    }
//                }
//            }

//            private void Allocate(string itemDefinitionId, IMyInventory inventory, VRage.MyFixedPoint amount)
//            {
//                Dictionary<IMyInventory, Allocation> allocationByInventory;
//                if(!AllocationsByInventoryByItemDefinitionId.TryGetValue(itemDefinitionId, out allocationByInventory))
//                {
//                    allocationByInventory = _program._inventoryAllocationDictionaryObjectPool.Get();
//                    AllocationsByInventoryByItemDefinitionId.Add(itemDefinitionId, allocationByInventory);
//                }
//                allocationByInventory.Add(inventory, new Allocation()
//                {
//                    Amount = amount
//                });
//            }

//            private readonly Program _program;

//            private readonly List<IMyInventory> Inventories
//                = new List<IMyInventory>();

//            private readonly Dictionary<string, List<InventoryRequest>> InventoryRequestsByItemDefinitionId
//                = new Dictionary<string, List<InventoryRequest>>();

//            private readonly Dictionary<string, VRage.MyFixedPoint> TotalAvailableAmountsByItemDefinitionId
//                = new Dictionary<string, VRage.MyFixedPoint>();

//            private readonly Dictionary<string, Dictionary<IMyInventory, Allocation>> AllocationsByInventoryByItemDefinitionId
//                = new Dictionary<string, Dictionary<IMyInventory, Allocation>>();
//        }
//    }
//}
