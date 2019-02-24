using System.Diagnostics.CodeAnalysis;

namespace IngameScript
{
    public partial class Program
    {
        [ExcludeFromCodeCoverage]
        public sealed partial class Deque<T> { }

        #if !MIXINS_TEST

        [ExcludeFromCodeCoverage]
        public partial struct BackgroundOperationResult { }

        [ExcludeFromCodeCoverage]
        public partial class BackgroundWorkerSettingsProvider { }

        [ExcludeFromCodeCoverage]
        public partial class BackgroundWorker { }

        [ExcludeFromCodeCoverage]
        public partial class BlockCollectionManager<T> { }

        [ExcludeFromCodeCoverage]
        public partial struct BlockCollectionResult { }

        [ExcludeFromCodeCoverage]
        public partial struct ConfigLine { }

        [ExcludeFromCodeCoverage]
        public partial class ConfigManager { }

        [ExcludeFromCodeCoverage]
        public partial class DateTimeProvider { }

        [ExcludeFromCodeCoverage]
        public partial class EchoProvider { }

        [ExcludeFromCodeCoverage]
        public partial class LoggerSettingsProvider { }

        [ExcludeFromCodeCoverage]
        public partial class Logger { }

        [ExcludeFromCodeCoverage]
        public partial class ManagedBlockCollectionHandler { }

        [ExcludeFromCodeCoverage]
        public partial struct ManagedBlockConfigLine { }

        [ExcludeFromCodeCoverage]
        public partial class ManagedBlockConfigManager { }

        [ExcludeFromCodeCoverage]
        public partial class ManagedBlockSettingsProvider { }

        [ExcludeFromCodeCoverage]
        public partial class ManagerSettingsProvider { }

        [ExcludeFromCodeCoverage]
        public partial class ObjectPool<T> { }

        [ExcludeFromCodeCoverage]
        public partial struct ParseResult { }

        #endif
    }
}
