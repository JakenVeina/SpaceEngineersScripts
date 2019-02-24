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
//        private struct BlockConfig
//        {
//            public bool Manage;

//            public bool Ignore;

//            public bool IgnoreInput;

//            public bool IgnoreOutput;

//            public List<ItemRequest> Requests;
//        }

//        private BlockConfig ParseBlockConfig(IMyTerminalBlock block)
//        {
//            var config = new BlockConfig()
//            {
//                Requests = _itemRequestListObjectPool.Get()
//            };

//            foreach (var configLine in GetConfigLines(block))
//            {
//                var configPieces = GetConfigPieces(configLine);
//                if ((configPieces.Length < 2) || (configPieces[0] != _scriptConfig.ConfigTag))
//                    continue;

//                switch (configPieces[1])
//                {
//                    case "manage":
//                        config.Manage = true;
//                        break;

//                    case "ignore":
//                        config.Ignore = true;
//                        break;

//                    case "ignore-input":
//                        config.IgnoreInput = true;
//                        break;

//                    case "ignore-output":
//                        config.IgnoreOutput = true;
//                        break;

//                    case "request":
//                        if (configPieces.Length < 3)
//                        {
//                            ReportBadConfigLine(configLine);
//                            break;
//                        }

//                        var amount = null as VRage.MyFixedPoint?;
//                        double doubleAmount;
//                        if ((configPieces.Length >= 4) && double.TryParse(configPieces[3], out doubleAmount))
//                            amount = (VRage.MyFixedPoint)doubleAmount;

//                        config.Requests.Add(new ItemRequest()
//                        {
//                            ItemDefinitionId = ItemDefinitionIdPrefix + configPieces[2],
//                            Amount = amount
//                        });
//                        break;

//                    default:
//                        ReportBadConfigLine(configLine);
//                        break;
//                }
//            }

//            return config;
//        }
//        private const string ItemDefinitionIdPrefix
//            = "MyObjectBuilder_";
//    }
//}
