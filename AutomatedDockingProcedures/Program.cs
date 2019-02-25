using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        public Program()
        {
            var backgroundWorkerSettingsProvider = new BackgroundWorkerSettingsProvider();
            var dateTimeProvider = new DateTimeProvider();
            var loggerSettingsProvider = new LoggerSettingsProvider();

            var configParseHandlers = new IConfigParseHandler[]
            {
                loggerSettingsProvider,
                backgroundWorkerSettingsProvider
            };
            var logger = new Logger(
                    dateTimeProvider,
                    loggerSettingsProvider);

            var backgroundWorker = new BackgroundWorker(
                    backgroundWorkerSettingsProvider,
                    dateTimeProvider,
                    Runtime);
            var configManager = new ConfigManager(
                     configParseHandlers,
                     logger,
                     Me);
            var echoProvider = new EchoProvider(Echo);

            _programManager = new ProgramManager(
                backgroundWorker,
                configManager,
                echoProvider,
                Runtime,
                logger);
        }

        public void Main(string argument)
            => _programManager.Run(argument);

        private readonly ProgramManager _programManager;
    }
}