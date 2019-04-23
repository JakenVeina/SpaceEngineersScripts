using System;
using System.Collections.Generic;

using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    public partial class Program
    {
        public interface IManagedBlockConfigParseHandler
        {
            void OnStarting();

            ParseResult OnParsing(ManagedBlockConfigLine configLine);

            void OnCompleted();
        }

        public interface IManagedBlockConfigManager
        {
            IBackgroundOperation MakeParseOperation(IMyTerminalBlock block);
        }

        public partial class ManagedBlockConfigManager : IManagedBlockConfigManager
        {
            public ManagedBlockConfigManager(
                ILogger logger,
                IReadOnlyList<IManagedBlockConfigParseHandler> managedBlockConfigParseHandlers,
                IManagerSettingsProvider managerSettingsProvider)
            {
                _logger = logger;
                _managedBlockConfigParseHandlers = managedBlockConfigParseHandlers;
                _managerSettingsProvider = managerSettingsProvider;

                _parseOperationPool = new ObjectPool<ParseOperation>(onFinished =>
                    new ParseOperation(this, onFinished));

                _parseLineOperationPool = new ObjectPool<ParseLineOperation>(onFinished =>
                    new ParseLineOperation(this, onFinished));
            }

            public IBackgroundOperation MakeParseOperation(IMyTerminalBlock block)
            {
                var parseOperation = _parseOperationPool.Get();

                parseOperation.Block = block;

                return parseOperation;
            }

            private readonly ILogger _logger;

            private readonly IReadOnlyList<IManagedBlockConfigParseHandler> _managedBlockConfigParseHandlers;

            private readonly IManagerSettingsProvider _managerSettingsProvider;

            private readonly ObjectPool<ParseOperation> _parseOperationPool;

            private readonly ObjectPool<ParseLineOperation> _parseLineOperationPool;

            private class ParseOperation : IBackgroundOperation, IDisposable
            {
                private static readonly char[] _lineSeparators
                    = new[] { '\n' };

                public ParseOperation(ManagedBlockConfigManager owner, Action onDisposed)
                {
                    _owner = owner;
                    _onDisposed = onDisposed;

                    Reset();
                }

                public IMyTerminalBlock Block;

                public BackgroundOperationResult Execute(Action<IBackgroundOperation> subOperationScheduler)
                    => _executeMethodsByState[(int)_state].Invoke(this, subOperationScheduler);

                public void Dispose()
                {
                    Reset();
                    _onDisposed.Invoke();
                }

                private void Reset()
                {
                    _state = OperationState.Initializing;
                }

                private static BackgroundOperationResult OnInitializing(ParseOperation @this, Action<IBackgroundOperation> subOperationScheduler)
                {
                    if (@this._owner._managedBlockConfigParseHandlers.Count == 0)
                        return BackgroundOperationResult.Completed;

                    @this._handlerIndex = 0;
                    @this._state = OperationState.Starting;
                    return BackgroundOperationResult.NotCompleted;
                }

                private static BackgroundOperationResult OnStarting(ParseOperation @this, Action<IBackgroundOperation> subOperationScheduler)
                {
                    @this._owner._managedBlockConfigParseHandlers[@this._handlerIndex].OnStarting();

                    if (++@this._handlerIndex >= @this._owner._managedBlockConfigParseHandlers.Count)
                    {
                        @this._lines = @this.Block.CustomData.Split(_lineSeparators, StringSplitOptions.RemoveEmptyEntries);

                        @this._lineIndex = 0;
                        @this._handlerIndex = 0;
                        @this._state = (@this._lines.Length == 0)
                            ? OperationState.Completing
                            : OperationState.Parsing;
                    }

                    return BackgroundOperationResult.NotCompleted;
                }

                private static BackgroundOperationResult OnParsing(ParseOperation @this, Action<IBackgroundOperation> subOperationScheduler)
                {
                    var parseLineOperation = @this._owner._parseLineOperationPool.Get();

                    parseLineOperation.Block = @this.Block;
                    parseLineOperation.Line = @this._lines[@this._lineIndex];

                    subOperationScheduler.Invoke(parseLineOperation);

                    if (++@this._lineIndex >= @this._lines.Length)
                    {
                        @this._state = OperationState.Completing;
                    }

                    return BackgroundOperationResult.NotCompleted;
                }

                private static BackgroundOperationResult OnCompleting(ParseOperation @this, Action<IBackgroundOperation> subOperationScheduler)
                {
                    @this._owner._managedBlockConfigParseHandlers[@this._handlerIndex].OnCompleted();

                    if (++@this._handlerIndex < @this._owner._managedBlockConfigParseHandlers.Count)
                        return BackgroundOperationResult.NotCompleted;

                    @this.Block = null;
                    @this._lines = null;
                    return BackgroundOperationResult.Completed;
                }

                private enum OperationState
                {
                    Initializing = 0,
                    Starting = 1,
                    Parsing = 2,
                    Completing = 3
                }

                private readonly ManagedBlockConfigManager _owner;

                private readonly Action _onDisposed;

                private OperationState _state;

                private int _handlerIndex;

                private string[] _lines;

                private int _lineIndex;

                private static readonly Func<ParseOperation, Action<IBackgroundOperation>, BackgroundOperationResult>[] _executeMethodsByState
                    = new Func<ParseOperation, Action<IBackgroundOperation>, BackgroundOperationResult>[]
                    {
                        OnInitializing,
                        OnStarting,
                        OnParsing,
                        OnCompleting
                    };
            }

            private class ParseLineOperation : IBackgroundOperation, IDisposable
            {
                private static readonly char[] _linePieceSeparators
                    = new[] { ':' };

                public ParseLineOperation(ManagedBlockConfigManager owner, Action onDisposed)
                {
                    _owner = owner;
                    _onDisposed = onDisposed;

                    Reset();
                }

                public IMyTerminalBlock Block;

                public string Line;

                public BackgroundOperationResult Execute(Action<IBackgroundOperation> subOperationScheduler)
                {
                    if (_linePieces == null)
                    {
                        _linePieces = Line.Split(_linePieceSeparators, StringSplitOptions.RemoveEmptyEntries);

                        if ((_linePieces.Length == 0) || (_linePieces[0] != $"[{_owner._managerSettingsProvider.Settings.BlockTag}]"))
                            return Complete();

                        if (_linePieces.Length < 2)
                        {
                            _owner._logger.AddLine($"Config Error:\n  Block:\"{Block.CustomName}\"\n  Line: \"{Line}\"\n  Usage: \"<BlockTag>:<option>[:<parameter>...]");
                            return Complete();
                        }

                        _handlerIndex = 0;
                    }

                    var parseResult = _owner._managedBlockConfigParseHandlers[_handlerIndex++].OnParsing(new ManagedBlockConfigLine(_linePieces));

                    if (parseResult.IsSuccess)
                        return Complete();

                    if (parseResult.IsError)
                    {
                        _owner._logger.AddLine($"Config Error:\n  Block:\"{Block.CustomName}\"\n  Line: \"{Line}\"\n  " + parseResult.Error);
                        return Complete();
                    }

                    if (_handlerIndex < _owner._managedBlockConfigParseHandlers.Count)
                        return BackgroundOperationResult.NotCompleted;

                    _owner._logger.AddLine($"Unknown Config Line:\n  Block: \"{Block.CustomName}\"\n  Line: \"{Line}\"");
                    return Complete();
                }

                public void Dispose()
                {
                    Reset();
                    _onDisposed.Invoke();
                }

                private void Reset()
                {
                    _linePieces = null;
                }

                private BackgroundOperationResult Complete()
                {
                    Block = null;
                    Line = null;
                    _linePieces = null;
                    return BackgroundOperationResult.Completed;
                }

                private readonly ManagedBlockConfigManager _owner;

                private readonly Action _onDisposed;

                private string[] _linePieces;

                private int _handlerIndex;
            }
        }
    }
}
