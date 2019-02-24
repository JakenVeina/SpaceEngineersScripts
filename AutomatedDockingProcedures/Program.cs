using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        public Program()
        {
            var dateTimeProvider = new DateTimeProvider();
            var loggerSettingsProvider = new LoggerSettingsProvider();

            var echoProvider = new EchoProvider(Echo);
            var logger = new Logger(
                dateTimeProvider,
                loggerSettingsProvider);

            _programManager = new ProgramManager(
                echoProvider,
                logger);
        }

        public void Main(string argument)
            => _programManager.Run(argument);

        private readonly ProgramManager _programManager;
    }
}