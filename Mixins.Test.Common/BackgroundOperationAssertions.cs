using static IngameScript.Program;

using Mixins.Test.Common;

namespace Shouldly
{
    public static class BackgroundOperationAssertions
    {
        public static void ShouldRunToCompletion(this IBackgroundOperation backgroundOperation)
            => backgroundOperation.ShouldRunToCompletion(new FakeBackgroundWorker());

        public static void ShouldRunToCompletion(this IBackgroundOperation backgroundOperation, FakeBackgroundWorker backgroundWorker)
            => backgroundWorker.ShouldCompleteOperation(backgroundOperation);

        public static void ShouldRunToCompletionIn(this IBackgroundOperation backgroundOperation, int expectedExecuteCount = 100)
            => backgroundOperation.ShouldRunToCompletionIn(new FakeBackgroundWorker(), expectedExecuteCount);

        public static void ShouldRunToCompletionIn(this IBackgroundOperation backgroundOperation, FakeBackgroundWorker backgroundWorker, int expectedExecuteCount)
            => backgroundWorker.ShouldCompleteOperationIn(backgroundOperation, expectedExecuteCount);
    }
}
