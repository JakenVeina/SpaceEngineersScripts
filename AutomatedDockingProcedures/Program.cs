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
            var dockingManagerSettingsProvider = new DockingManagerSettingsProvider();

            var batteryBlockCollectionHandler = new BatteryBlockCollectionHandler(
                    batteryBlockManager,
                    logger,
                    dockingManagerSettingsProvider);
            var connectorCollectionHandler = new ConnectorCollectionHandler(
                    connectorManager,
                    logger);
            var functionalBlockCollectionHandler = new FunctionalBlockCollectionHandler(
                    functionalBlockManager,
                    logger,
                    dockingManagerSettingsProvider);
            var gasTankCollectionHandler = new GasTankCollectionHandler(
                    gasTankManager,
                    logger,
                    dockingManagerSettingsProvider);
            var landingGearCollectionHandler = new LandingGearCollectionHandler(
                    landingGearManager,
                    logger,
                    dockingManagerSettingsProvider);
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
                dockingManagerSettingsProvider,
                programReloadHandler
            };

            var configManager = new ConfigManager(
                     configParseHandlers,
                     logger,
                     Me);
            var dockingManager = new DockingManager(
                     logger,
                     batteryBlockManager,
                     connectorManager,
                     functionalBlockManager,
                     gasTankManager,
                     landingGearManager);
            var echoProvider = new EchoProvider(Echo);

            _programManager = new ProgramManager(
                backgroundWorker,
                configManager,
                dockingManager,
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