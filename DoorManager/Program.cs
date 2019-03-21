using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    public partial class Program : MyGridProgram
    {
        public Program()
        {
            var doorManagerSettingsProvider = new DoorManagerSettingsProvider();

            var dateTimeProvider = new DateTimeProvider();
            var loggerSettingsProvider = new LoggerSettingsProvider();
            var managedBlockSettingsProvider = new ManagedBlockSettingsProvider();
            var managedDoorSettingsProvider = new ManagedDoorSettingsProvider(
                    doorManagerSettingsProvider);

            var logger = new Logger(
                    dateTimeProvider,
                    loggerSettingsProvider);
            var managedBlockConfigParseHandlers = new IManagedBlockConfigParseHandler[]
            {
                managedBlockSettingsProvider,
                managedDoorSettingsProvider
            };
            var managerSettingsProvider = new ManagerSettingsProvider()
            {
                DefaultBlockTag = "DoorManager"
            };

            var doorManager = new DoorManager(
                    dateTimeProvider,
                    doorManagerSettingsProvider,
                    logger);
            var managedBlockConfigManager = new ManagedBlockConfigManager(
                    logger,
                    managedBlockConfigParseHandlers,
                    managerSettingsProvider);

            var managedBlockCollectionHandler = new ManagedBlockCollectionHandler(
                    managedBlockConfigManager,
                    managedBlockSettingsProvider,
                    managerSettingsProvider,
                    Me);
            var managedDoorCollectionHandler = new ManagedDoorCollectionHandler(
                    doorManager,
                    logger,
                    managedDoorSettingsProvider);

            var backgroundWorkerSettingsProvider = new BackgroundWorkerSettingsProvider();
            var blockCollectionHandlers = new IBlockCollectionHandler[]
            {
                managedBlockCollectionHandler,
                managedDoorCollectionHandler
            };

            var backgroundWorker = new BackgroundWorker(
                    backgroundWorkerSettingsProvider,
                    dateTimeProvider,
                    Runtime);
            var blockCollectionManager = new BlockCollectionManager<IMyDoor>(
                    blockCollectionHandlers,
                    GridTerminalSystem);
            var programSettingsProvider = new ProgramSettingsProvider();

            var programReloadHandler = new ProgramReloadHandler(
                    backgroundWorker,
                    blockCollectionManager,
                    doorManager,
                    programSettingsProvider);

            var configParseHandlers = new IConfigParseHandler[]
            {
                loggerSettingsProvider,
                backgroundWorkerSettingsProvider,
                managerSettingsProvider,
                doorManagerSettingsProvider,
                programSettingsProvider,
                programReloadHandler
            };

            var configManager = new ConfigManager(
                    configParseHandlers,
                    logger,
                    Me);
            var echoProvider = new EchoProvider(
                    Echo);

            _programManager = new ProgramManager(
                backgroundWorker,
                configManager,
                doorManager,
                echoProvider,
                Runtime,
                logger);

            _programManager.Run("reload");
        }

        public void Main(string argument)
            => _programManager.Run(argument);

        private readonly ProgramManager _programManager;
    }
}