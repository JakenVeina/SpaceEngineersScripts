﻿using System;
using System.Collections.Generic;

using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public class ProgramManager
        {
            public ProgramManager(
                IBackgroundWorker backgroundWorker,
                IConfigManager configManager,
                IDockingManager dockingManager,
                IEchoProvider echoProvider,
                IMyGridProgramRuntimeInfo gridProgramRuntimeInfo,
                ILogger logger)
            {
                _backgroundWorker = backgroundWorker;
                _configManager = configManager;
                _dockingManager = dockingManager;
                _echoProvider = echoProvider;
                _gridProgramRuntimeInfo = gridProgramRuntimeInfo;
                _logger = logger;
            }

            public void Run(string argument)
            {
                Action<ProgramManager> runAction;
                if (_runActionsByArgument.TryGetValue(argument.ToLower(), out runAction))
                    runAction.Invoke(this);
                else
                    _logger.AddLine($"Invalid argument: \"{argument}\"");

                _echoProvider.Echo(_logger.Render());
            }

            private static void DoReload(ProgramManager @this)
            {
                @this._backgroundWorker.ScheduleOperation(@this._configManager.MakeParseOperation());

                @this._gridProgramRuntimeInfo.UpdateFrequency |= UpdateFrequency.Once;
            }

            private static void DoDock(ProgramManager @this)
            {
                @this._backgroundWorker.ScheduleOperation(@this._dockingManager.MakeDockOperation());

                @this._gridProgramRuntimeInfo.UpdateFrequency |= UpdateFrequency.Once;
            }

            private static void DoUndock(ProgramManager @this)
            {
                @this._backgroundWorker.ScheduleOperation(@this._dockingManager.MakeUndockOperation());

                @this._gridProgramRuntimeInfo.UpdateFrequency |= UpdateFrequency.Once;
            }

            private static void DoToggle(ProgramManager @this)
            {
                @this._backgroundWorker.ScheduleOperation(@this._dockingManager.MakeToggleOperation());

                @this._gridProgramRuntimeInfo.UpdateFrequency |= UpdateFrequency.Once;
            }

            private static void DoRun(ProgramManager @this)
                => @this._backgroundWorker.ExecuteOperations();

            private readonly IBackgroundWorker _backgroundWorker;

            private readonly IConfigManager _configManager;

            private readonly IDockingManager _dockingManager;

            private readonly IEchoProvider _echoProvider;

            private readonly IMyGridProgramRuntimeInfo _gridProgramRuntimeInfo;

            private readonly ILogger _logger;

            private static readonly Dictionary<string, Action<ProgramManager>> _runActionsByArgument
                = new Dictionary<string, Action<ProgramManager>>()
                {
                    { "reload", DoReload },
                    { "dock",   DoDock   },
                    { "undock", DoUndock },
                    { "toggle", DoToggle },
                    { "run",    DoRun    },
                    { "",       DoRun    }
                };
        }
    }
}
