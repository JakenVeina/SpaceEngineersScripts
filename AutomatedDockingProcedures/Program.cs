using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        public Program()
        {
            var dateTimeProvider = new DateTimeProvider();
            var loggerSettingsProvider = new LoggerSettingsProvider();
            var managedBlockSettingsProvider = new ManagedBlockSettingsProvider();

            var logger = new Logger(
                    dateTimeProvider,
                    loggerSettingsProvider);
            var managedBlockConfigParseHandlers = new IManagedBlockConfigParseHandler[]
            {
                managedBlockSettingsProvider
            };
            var managerSettingsProvider = new ManagerSettingsProvider();

            var batteryBlockManager = new BatteryBlockManager();
            var connectorManager = new ConnectorManager();
            var functionalBlockManager = new FunctionalBlockManager();
            var gasTankManager = new GasTankManager();
            var landingGearManager = new LandingGearManager();
            var managedBlockConfigManager = new ManagedBlockConfigManager(
                    logger,
                    managedBlockConfigParseHandlers,
                    managerSettingsProvider);
            var programSettingsProvider = new ProgramSettingsProvider();

            var batteryBlockCollectionHandler = new BatteryBlockCollectionHandler(
                    batteryBlockManager,
                    logger,
                    programSettingsProvider);
            var connectorCollectionHandler = new ConnectorCollectionHandler(
                    connectorManager,
                    logger);
            var functionalBlockCollectionHandler = new FunctionalBlockCollectionHandler(
                    functionalBlockManager,
                    logger,
                    programSettingsProvider);
            var gasTankCollectionHandler = new GasTankCollectionHandler(
                    gasTankManager,
                    logger,
                    programSettingsProvider);
            var landingGearCollectionHandler = new LandingGearCollectionHandler(
                    landingGearManager,
                    logger,
                    programSettingsProvider);
            var managedBlockCollectionHandler = new ManagedBlockCollectionHandler(
                    managedBlockConfigManager,
                    managedBlockSettingsProvider,
                    managerSettingsProvider,
                    Me);

            var backgroundWorkerSettingsProvider = new BackgroundWorkerSettingsProvider();
            var blockCollectionHandlers = new IBlockCollectionHandler[]
            {
                managedBlockCollectionHandler,
                batteryBlockCollectionHandler,
                connectorCollectionHandler,
                functionalBlockCollectionHandler,
                gasTankCollectionHandler,
                landingGearCollectionHandler
            };

            var backgroundWorker = new BackgroundWorker(
                    backgroundWorkerSettingsProvider,
                    dateTimeProvider,
                    Runtime);
            var blockCollectionManager = new BlockCollectionManager<IMyTerminalBlock>(
                    blockCollectionHandlers,
                    GridTerminalSystem);

            var programReloadHandler = new ProgramReloadHandler(
                    backgroundWorker,
                    blockCollectionManager);

            var configParseHandlers = new IConfigParseHandler[]
            {
                backgroundWorkerSettingsProvider,
                loggerSettingsProvider,
                managerSettingsProvider,
                programSettingsProvider,
                programReloadHandler
            };

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