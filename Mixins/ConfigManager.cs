using System;
using System.Collections.Generic;

using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    public partial class Program
    {
        public interface IConfigParseHandler
        {
            void OnStarting();

            ParseResult OnParsing(ConfigLine configLine);

            void OnCompleted();
        }

        public interface IConfigManager
        {
            IBackgroundOperation MakeParseOperation();
        }

        public partial class ConfigManager : IConfigManager
        {
            public ConfigManager(
                IReadOnlyList<IConfigParseHandler> configParseHandlers,
                ILogger logger,
                IMyProgrammableBlock programmableBlock)
            {
                _configParseHandlers = configParseHandlers;
                _logger = logger;
                _programmableBlock = programmableBlock;

                _parseOperationPool = new ObjectPool<ParseOperation>(onDisposed =>
                    new ParseOperation(this, onDisposed));

                _parseLineOperationPool = new ObjectPool<ParseLineOperation>(onDisposed =>
                    new ParseLineOperation(this, onDisposed));
            }

            public IBackgroundOperation MakeParseOperation()
                => _parseOperationPool.Get();

            private readonly IReadOnlyList<IConfigParseHandler> _configParseHandlers;

            private readonly ILogger _logger;

            private readonly IMyProgrammableBlock _programmableBlock;

            private readonly ObjectPool<ParseOperation> _parseOperationPool;

            private readonly ObjectPool<ParseLineOperation> _parseLineOperationPool;

            private class ParseOperation : IBackgroundOperation, IDisposable
            {
                private static readonly char[] _lineSeparators
                    = new[] { '\n' };

                public ParseOperation(ConfigManager owner, Action onDisposed)
                {
                    _owner = owner;
                    _onDisposed = onDisposed;

                    Reset();
                }

                public BackgroundOperationResult Execute(Action<IBackgroundOperation> subOperationScheduler)
                    => _executeMethodsByState[(int)_state].Invoke(this, subOperationScheduler);

                public void Dispose()
                {
                    Reset();
                    _onDisposed.Invoke();
                }

                public void Reset()
                {
                    _state = OperationState.Initializing;
                }

                private static BackgroundOperationResult OnInitializing(ParseOperation @this, Action<IBackgroundOperation> subOperationScheduler)
                {
                    if (@this._owner._configParseHandlers.Count == 0)
                        return BackgroundOperationResult.Completed;

                    @this._handlerIndex = 0;
                    @this._state = OperationState.Starting;
                    return BackgroundOperationResult.NotCompleted;
                }

                private static BackgroundOperationResult OnStarting(ParseOperation @this, Action<IBackgroundOperation> subOperationScheduler)
                {
                    @this._owner._configParseHandlers[@this._handlerIndex].OnStarting();

                    if (++@this._handlerIndex >= @this._owner._configParseHandlers.Count)
                    {
                        @this._lines = @this._owner._programmableBlock.CustomData.Split(_lineSeparators, StringSplitOptions.RemoveEmptyEntries);

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
                    @this._owner._configParseHandlers[@this._handlerIndex].OnCompleted();

                    if (++@this._handlerIndex < @this._owner._configParseHandlers.Count)
                        return BackgroundOperationResult.NotCompleted;

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

                private readonly ConfigManager _owner;

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
                private static readonly char[] _lineParamsSeparators
                    = new[] { ':' };

                public ParseLineOperation(ConfigManager owner, Action onDisposed)
                {
                    _owner = owner;
                    _onDisposed = onDisposed;

                    Reset();
                }

                public string Line;

                public BackgroundOperationResult Execute(Action<IBackgroundOperation> subOperationScheduler)
                {
                    if (_linePieces == null)
                    {
                        _linePieces = Line.Split(_lineParamsSeparators, StringSplitOptions.RemoveEmptyEntries);

                        if (_linePieces.Length == 0)
                            return Complete();

                        _handlerIndex = 0;
                    }

                    var parseResult = _owner._configParseHandlers[_handlerIndex++].OnParsing(new ConfigLine(_linePieces));

                    if (parseResult.IsSuccess)
                        return Complete();

                    if (parseResult.IsError)
                    {
                        _owner._logger.AddLine($"Config Error:\n  Line: \"{Line}\"\n  {parseResult.Error}");
                        return Complete();
                    }

                    if (_handlerIndex < _owner._configParseHandlers.Count)
                        return BackgroundOperationResult.NotCompleted;

                    _owner._logger.AddLine($"Unknown Config Line:\n  Line: \"{Line}\"");
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
                    Line = null;
                    _linePieces = null;
                    return BackgroundOperationResult.Completed;
                }

                private readonly ConfigManager _owner;

                private readonly Action _onDisposed;

                private string[] _linePieces;

                private int _handlerIndex;
            }
        }
    }
}
