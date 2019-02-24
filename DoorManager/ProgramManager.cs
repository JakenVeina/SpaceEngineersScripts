using System;
using System.Collections.Generic;

using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    public partial class Program
    {
        public class ProgramManager
        {
            public ProgramManager(
                IBackgroundWorker backgroundWorker,
                IConfigManager configManager,
                IDoorManager doorManager,
                IEchoProvider echoProvider,
                IMyGridProgramRuntimeInfo gridProgramRuntimeInfo,
                ILogger logger)
            {
                _backgroundWorker = backgroundWorker;
                _configManager = configManager;
                _doorManager = doorManager;
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

                //_logger.AddLine($"Executed {_gridProgramRuntimeInfo.CurrentInstructionCount} instructions");

                _echoProvider.Echo(_logger.Render());
            }

            private static void DoReload(ProgramManager @this)
            {
                @this._backgroundWorker.ScheduleOperation(@this._configManager.MakeParseOperation());

                @this._gridProgramRuntimeInfo.UpdateFrequency |= UpdateFrequency.Once;
            }

            private static void DoLockdown(ProgramManager @this)
            {
                @this._doorManager.IsLockdownEnabled = true;

                @this._gridProgramRuntimeInfo.UpdateFrequency |= UpdateFrequency.Once;
            }

            private static void DoRelease(ProgramManager @this)
            {
                @this._doorManager.IsLockdownEnabled = false;

                @this._gridProgramRuntimeInfo.UpdateFrequency |= UpdateFrequency.Once;
            }

            private static void DoStats(ProgramManager @this)
            {
                @this._logger.AddLine($"Door Management Stats:\n  Managing {@this._doorManager.DoorCount} doors");
            }

            private static void DoStop(ProgramManager @this)
                => @this._gridProgramRuntimeInfo.UpdateFrequency = UpdateFrequency.None;

            private static void DoRun(ProgramManager @this)
                => @this._backgroundWorker.ExecuteOperations();

            private readonly IBackgroundWorker _backgroundWorker;

            private readonly IConfigManager _configManager;

            private readonly IDoorManager _doorManager;

            private readonly IEchoProvider _echoProvider;

            private readonly IMyGridProgramRuntimeInfo _gridProgramRuntimeInfo;

            private readonly ILogger _logger;

            private static readonly Dictionary<string, Action<ProgramManager>> _runActionsByArgument
                = new Dictionary<string, Action<ProgramManager>>()
                {
                    { "reload",   DoReload   },
                    { "lockdown", DoLockdown },
                    { "release",  DoRelease  },
                    { "stats",    DoStats    },
                    { "stop",     DoStop     },
                    { "run",      DoRun      },
                    { "",         DoRun      }
                };
        }
    }
}
