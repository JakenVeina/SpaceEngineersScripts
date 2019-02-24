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
//        private void ReportBadConfigLine(string configLine)
//            => Log($"Invalid config entry: \"{configLine}\"");

//        private ScriptConfig ParseScriptConfig()
//        {
//            var config = new ScriptConfig()
//            {
//                ConfigTag = "InventoryManager",
//                ProcessInterval = TimeSpan.FromSeconds(1),
//                CleanInterval = TimeSpan.FromMinutes(1),
//                InstructionsPerCycle = 25000,
//                LogSize = 10
//            };

//            foreach (var configLine in GetConfigLines(Me))
//            {
//                var configPieces = GetConfigPieces(configLine);
//                if (!configPieces.Any())
//                    continue;

//                switch (configPieces[0])
//                {
//                    case "config-tag":
//                        if (configPieces.Length == 2)
//                            config.ConfigTag = configPieces[1];
//                        else
//                            ReportBadConfigLine(configLine);
//                        break;

//                    case "instructions-per-cycle":
//                        int instructionsPerCycle;
//                        if ((configPieces.Length == 2) && int.TryParse(configPieces[1], out instructionsPerCycle) && (instructionsPerCycle > 0))
//                            config.InstructionsPerCycle = instructionsPerCycle;
//                        else
//                            ReportBadConfigLine(configLine);
//                        break;

//                    case "process-interval":
//                        int processIntervalMs;
//                        if ((configPieces.Length == 2) && int.TryParse(configPieces[1], out processIntervalMs) && (processIntervalMs > 1))
//                            config.ProcessInterval = TimeSpan.FromMilliseconds(processIntervalMs);
//                        else
//                            ReportBadConfigLine(configLine);
//                        break;

//                    case "auto-manage-this-grid":
//                        config.AutoManageThisGrid = true;
//                        break;

//                    case "manage-other-grids":
//                        config.ManageOtherGrids = true;
//                        break;

//                    case "auto-remap-grids":
//                        config.AutoRemapGrids = true;
//                        break;

//                    case "auto-manage-other-grids":
//                        config.AutoManageOtherGrids = true;
//                        break;

//                    case "ignore-production-inputs":
//                        config.IgnoreProductionInputs = true;
//                        break;

//                    case "log-size":
//                        int logSize;
//                        if ((configPieces.Length == 2) && int.TryParse(configPieces[1], out logSize) && (logSize >= 0))
//                            config.LogSize = logSize;
//                        else
//                            ReportBadConfigLine(configLine);
//                        break;

//                    default:
//                        ReportBadConfigLine(configLine);
//                        break;
//                }
//            }

//            return config;
//        }

//        private struct ScriptConfig
//        {
//            public string ConfigTag;

//            public int InstructionsPerCycle;

//            public TimeSpan ProcessInterval;

//            public TimeSpan CleanInterval;

//            public bool AutoManageThisGrid;

//            public bool ManageOtherGrids;

//            public bool AutoRemapGrids;

//            public bool AutoManageOtherGrids;

//            public bool IgnoreProductionInputs;

//            public int LogSize;
//        }
//    }
//}
