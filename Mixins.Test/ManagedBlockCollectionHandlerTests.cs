using System;
using System.Linq;

using Sandbox.ModAPI.Ingame;

using Moq;
using NUnit.Framework;
using Shouldly;

using static IngameScript.Program;

using Mixins.Test.Common;

namespace Mixins.Test
{
    [TestFixture]
    public class ManagedBlockCollectionHandlerTests
    {
        #region Test Context

        private class TestContext
        {
            public bool ManageOtherGrids;

            public bool AutoManageThisGrid;

            public bool AutoManageOtherGrids;

            public bool IsBlockSameGrid;

            public bool IsBlockIgnored;

            public bool IsBlockManaged;

            public TestContext()
            {
                MockParseOperation = new Mock<IBackgroundOperation>();
                MockParseOperation
                    .Setup(x => x.Execute(It.IsAny<Action<IBackgroundOperation>>()))
                    .Returns(BackgroundOperationResult.Completed);

                MockManagedBlockConfigManager = new Mock<IManagedBlockConfigManager>();
                MockManagedBlockConfigManager
                    .Setup(x => x.MakeParseOperation(It.IsAny<IMyTerminalBlock>()))
                    .Returns(MockParseOperation.Object);

                MockManagedBlockSettingsProvider = new Mock<IManagedBlockSettingsProvider>();
                MockManagedBlockSettingsProvider
                    .Setup(x => x.Settings)
                    .Returns(() => new ManagedBlockSettings()
                    {
                        Ignore = IsBlockIgnored,
                        Manage = IsBlockManaged
                    });

                MockManagerSettingsProvider = new Mock<IManagerSettingsProvider>();
                MockManagerSettingsProvider
                    .Setup(x => x.Settings)
                    .Returns(() => new ManagerSettings()
                    {
                        ManageOtherGrids = ManageOtherGrids,
                        AutoManageThisGrid = AutoManageThisGrid,
                        AutoManageOtherGrids = AutoManageOtherGrids
                    });

                var gridEntityId = 1;

                MockProgrammableBlock = new Mock<IMyProgrammableBlock>();
                MockProgrammableBlock
                    .Setup(x => x.CubeGrid.EntityId)
                    .Returns(gridEntityId);

                Uut = new ManagedBlockCollectionHandler(
                    MockManagedBlockConfigManager.Object,
                    MockManagedBlockSettingsProvider.Object,
                    MockManagerSettingsProvider.Object,
                    MockProgrammableBlock.Object);

                MockBlock = new Mock<IMyTerminalBlock>();
                MockBlock
                    .Setup(x => x.CubeGrid.EntityId)
                    .Returns(() => IsBlockSameGrid
                        ? gridEntityId
                        : gridEntityId + 1);
            }

            public readonly Mock<IManagedBlockConfigManager> MockManagedBlockConfigManager;

            public readonly Mock<IManagedBlockSettingsProvider> MockManagedBlockSettingsProvider;

            public readonly Mock<IManagerSettingsProvider> MockManagerSettingsProvider;

            public readonly Mock<IMyProgrammableBlock> MockProgrammableBlock;

            public readonly ManagedBlockCollectionHandler Uut;

            public readonly Mock<IBackgroundOperation> MockParseOperation;

            public readonly Mock<IMyTerminalBlock> MockBlock;
        }

        #endregion Test Context

        #region Test Cases

        public struct CollectBlockTestCase
        {
            public bool ManageOtherGrids;
            public bool AutoManageThisGrid;
            public bool AutoManageOtherGrids;
            public bool IsBlockSameGrid;
            public bool IsBlockIgnored;
            public bool IsBlockManaged;
        }

        public static readonly CollectBlockTestCase[] BlockShouldBeManagedTestCases
            = new[]
            {
                new CollectBlockTestCase() { ManageOtherGrids = false, AutoManageThisGrid = false, AutoManageOtherGrids = false, IsBlockSameGrid = true,  IsBlockIgnored = false, IsBlockManaged = true  },
                new CollectBlockTestCase() { ManageOtherGrids = false, AutoManageThisGrid = false, AutoManageOtherGrids = true,  IsBlockSameGrid = true,  IsBlockIgnored = false, IsBlockManaged = true  },
                new CollectBlockTestCase() { ManageOtherGrids = false, AutoManageThisGrid = true,  AutoManageOtherGrids = false, IsBlockSameGrid = true,  IsBlockIgnored = false, IsBlockManaged = false },
                new CollectBlockTestCase() { ManageOtherGrids = false, AutoManageThisGrid = true,  AutoManageOtherGrids = false, IsBlockSameGrid = true,  IsBlockIgnored = false, IsBlockManaged = true  },
                new CollectBlockTestCase() { ManageOtherGrids = false, AutoManageThisGrid = true,  AutoManageOtherGrids = true,  IsBlockSameGrid = true,  IsBlockIgnored = false, IsBlockManaged = false },
                new CollectBlockTestCase() { ManageOtherGrids = false, AutoManageThisGrid = true,  AutoManageOtherGrids = true,  IsBlockSameGrid = true,  IsBlockIgnored = false, IsBlockManaged = true  },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = false, AutoManageOtherGrids = false, IsBlockSameGrid = false, IsBlockIgnored = false, IsBlockManaged = true  },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = false, AutoManageOtherGrids = false, IsBlockSameGrid = true,  IsBlockIgnored = false, IsBlockManaged = true  },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = false, AutoManageOtherGrids = true,  IsBlockSameGrid = false, IsBlockIgnored = false, IsBlockManaged = false },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = false, AutoManageOtherGrids = true,  IsBlockSameGrid = false, IsBlockIgnored = false, IsBlockManaged = true  },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = false, AutoManageOtherGrids = true,  IsBlockSameGrid = true,  IsBlockIgnored = false, IsBlockManaged = true  },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = true,  AutoManageOtherGrids = false, IsBlockSameGrid = false, IsBlockIgnored = false, IsBlockManaged = true  },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = true,  AutoManageOtherGrids = false, IsBlockSameGrid = true,  IsBlockIgnored = false, IsBlockManaged = false },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = true,  AutoManageOtherGrids = false, IsBlockSameGrid = true,  IsBlockIgnored = false, IsBlockManaged = true  },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = true,  AutoManageOtherGrids = true,  IsBlockSameGrid = false, IsBlockIgnored = false, IsBlockManaged = false },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = true,  AutoManageOtherGrids = true,  IsBlockSameGrid = false, IsBlockIgnored = false, IsBlockManaged = true  },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = true,  AutoManageOtherGrids = true,  IsBlockSameGrid = true,  IsBlockIgnored = false, IsBlockManaged = false },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = true,  AutoManageOtherGrids = true,  IsBlockSameGrid = true,  IsBlockIgnored = false, IsBlockManaged = true  }
            };

        public static readonly CollectBlockTestCase[] BlockShouldNotBeManagedTestCases
            = new[]
            {
                new CollectBlockTestCase() { ManageOtherGrids = false, AutoManageThisGrid = false, AutoManageOtherGrids = false, IsBlockSameGrid = true,  IsBlockIgnored = false, IsBlockManaged = false },
                new CollectBlockTestCase() { ManageOtherGrids = false, AutoManageThisGrid = false, AutoManageOtherGrids = false, IsBlockSameGrid = true,  IsBlockIgnored = true,  IsBlockManaged = false },
                new CollectBlockTestCase() { ManageOtherGrids = false, AutoManageThisGrid = false, AutoManageOtherGrids = false, IsBlockSameGrid = true,  IsBlockIgnored = true,  IsBlockManaged = true  },
                new CollectBlockTestCase() { ManageOtherGrids = false, AutoManageThisGrid = false, AutoManageOtherGrids = true,  IsBlockSameGrid = true,  IsBlockIgnored = false, IsBlockManaged = false },
                new CollectBlockTestCase() { ManageOtherGrids = false, AutoManageThisGrid = false, AutoManageOtherGrids = true,  IsBlockSameGrid = true,  IsBlockIgnored = true,  IsBlockManaged = false },
                new CollectBlockTestCase() { ManageOtherGrids = false, AutoManageThisGrid = false, AutoManageOtherGrids = true,  IsBlockSameGrid = true,  IsBlockIgnored = true,  IsBlockManaged = true  },
                new CollectBlockTestCase() { ManageOtherGrids = false, AutoManageThisGrid = true,  AutoManageOtherGrids = false, IsBlockSameGrid = true,  IsBlockIgnored = true,  IsBlockManaged = false },
                new CollectBlockTestCase() { ManageOtherGrids = false, AutoManageThisGrid = true,  AutoManageOtherGrids = false, IsBlockSameGrid = true,  IsBlockIgnored = true,  IsBlockManaged = true  },
                new CollectBlockTestCase() { ManageOtherGrids = false, AutoManageThisGrid = true,  AutoManageOtherGrids = true,  IsBlockSameGrid = true,  IsBlockIgnored = true,  IsBlockManaged = false },
                new CollectBlockTestCase() { ManageOtherGrids = false, AutoManageThisGrid = true,  AutoManageOtherGrids = true,  IsBlockSameGrid = true,  IsBlockIgnored = true,  IsBlockManaged = true  },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = false, AutoManageOtherGrids = false, IsBlockSameGrid = false, IsBlockIgnored = false, IsBlockManaged = false },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = false, AutoManageOtherGrids = false, IsBlockSameGrid = false, IsBlockIgnored = true,  IsBlockManaged = false },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = false, AutoManageOtherGrids = false, IsBlockSameGrid = false, IsBlockIgnored = true,  IsBlockManaged = true  },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = false, AutoManageOtherGrids = false, IsBlockSameGrid = true,  IsBlockIgnored = false, IsBlockManaged = false },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = false, AutoManageOtherGrids = false, IsBlockSameGrid = true,  IsBlockIgnored = true,  IsBlockManaged = false },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = false, AutoManageOtherGrids = false, IsBlockSameGrid = true,  IsBlockIgnored = true,  IsBlockManaged = true  },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = false, AutoManageOtherGrids = true,  IsBlockSameGrid = false, IsBlockIgnored = true,  IsBlockManaged = false },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = false, AutoManageOtherGrids = true,  IsBlockSameGrid = false, IsBlockIgnored = true,  IsBlockManaged = true  },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = false, AutoManageOtherGrids = true,  IsBlockSameGrid = true,  IsBlockIgnored = false, IsBlockManaged = false },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = false, AutoManageOtherGrids = true,  IsBlockSameGrid = true,  IsBlockIgnored = true,  IsBlockManaged = false },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = false, AutoManageOtherGrids = true,  IsBlockSameGrid = true,  IsBlockIgnored = true,  IsBlockManaged = true  },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = true,  AutoManageOtherGrids = false, IsBlockSameGrid = false, IsBlockIgnored = false, IsBlockManaged = false },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = true,  AutoManageOtherGrids = false, IsBlockSameGrid = false, IsBlockIgnored = true,  IsBlockManaged = false },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = true,  AutoManageOtherGrids = false, IsBlockSameGrid = false, IsBlockIgnored = true,  IsBlockManaged = true  },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = true,  AutoManageOtherGrids = false, IsBlockSameGrid = true,  IsBlockIgnored = true,  IsBlockManaged = false },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = true,  AutoManageOtherGrids = false, IsBlockSameGrid = true,  IsBlockIgnored = true,  IsBlockManaged = true  },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = true,  AutoManageOtherGrids = true,  IsBlockSameGrid = false, IsBlockIgnored = true,  IsBlockManaged = false },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = true,  AutoManageOtherGrids = true,  IsBlockSameGrid = false, IsBlockIgnored = true,  IsBlockManaged = true  },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = true,  AutoManageOtherGrids = true,  IsBlockSameGrid = true,  IsBlockIgnored = true,  IsBlockManaged = false },
                new CollectBlockTestCase() { ManageOtherGrids = true,  AutoManageThisGrid = true,  AutoManageOtherGrids = true,  IsBlockSameGrid = true,  IsBlockIgnored = true,  IsBlockManaged = true  }
            };

        #endregion Test Cases

        #region OnStarting() Tests

        [Test]
        public void OnStarting_Always_DoesNothing()
        {
            var testContext = new TestContext();

            testContext.Uut.OnStarting();

            testContext.MockManagedBlockConfigManager
                .Invocations.ShouldBeEmpty();

            testContext.MockManagedBlockSettingsProvider
                .Invocations.ShouldBeEmpty();

            testContext.MockManagerSettingsProvider
                .Invocations.ShouldBeEmpty();

            testContext.MockProgrammableBlock
                .Invocations.ShouldBeEmpty();
        }

        #endregion OnStarting() Tests

        #region MakeCollectBlockOperation() Tests

        [Test]
        public void MakeCollectBlockOperation_BlockIsOtherGridAndOtherGridIsNotManaged_CompletesImmediately()
        {
            var testContext = new TestContext()
            {
                ManageOtherGrids = false,
                IsBlockSameGrid = false
            };
            var mockBackgroundWorker = new FakeBackgroundWorker();

            var result = testContext.Uut.MakeCollectBlockOperation(testContext.MockBlock.Object);

            result
                .ShouldRunToCompletionIn(mockBackgroundWorker, 1);

            mockBackgroundWorker.MockSubOperationScheduler
                .ShouldNotHaveReceived(x => x(It.IsAny<IBackgroundOperation>()));

            testContext.MockManagedBlockConfigManager
                .Invocations.Count.ShouldBe(0);

            result.Result.IsSkipped.ShouldBeTrue();
        }

        [TestCaseSource(nameof(BlockShouldBeManagedTestCases))]
        public void MakeCollectBlockOperation_BlockShouldBeManaged_ResultIsIgnored(CollectBlockTestCase testCase)
        {
            var testContext = new TestContext()
            {
                ManageOtherGrids     = testCase.ManageOtherGrids,
                AutoManageThisGrid   = testCase.AutoManageThisGrid,
                AutoManageOtherGrids = testCase.AutoManageOtherGrids,
                IsBlockSameGrid      = testCase.IsBlockSameGrid,
                IsBlockIgnored       = testCase.IsBlockIgnored,
                IsBlockManaged       = testCase.IsBlockManaged
            };
            var mockBackgroundWorker = new FakeBackgroundWorker();

            var result = testContext.Uut.MakeCollectBlockOperation(testContext.MockBlock.Object);

            result
                .ShouldRunToCompletion(mockBackgroundWorker);

            testContext.MockManagedBlockConfigManager
                .ShouldHaveReceived(x => x.MakeParseOperation(testContext.MockBlock.Object), 1);

            mockBackgroundWorker.MockSubOperationScheduler
                .ShouldHaveReceived(x => x(testContext.MockParseOperation.Object), 1);

            result.Result.IsIgnored.ShouldBeTrue();
        }

        [TestCaseSource(nameof(BlockShouldNotBeManagedTestCases))]
        public void MakeCollectBlockOperation_BlockShouldNotBeManaged_ResultIsSkipped(CollectBlockTestCase testCase)
        {
            var testContext = new TestContext()
            {
                ManageOtherGrids     = testCase.ManageOtherGrids,
                AutoManageThisGrid   = testCase.AutoManageThisGrid,
                AutoManageOtherGrids = testCase.AutoManageOtherGrids,
                IsBlockSameGrid      = testCase.IsBlockSameGrid,
                IsBlockIgnored       = testCase.IsBlockIgnored,
                IsBlockManaged       = testCase.IsBlockManaged
            };
            var mockBackgroundWorker = new FakeBackgroundWorker();

            var result = testContext.Uut.MakeCollectBlockOperation(testContext.MockBlock.Object);

            result
                .ShouldRunToCompletion(mockBackgroundWorker);

            testContext.MockManagedBlockConfigManager
                .ShouldHaveReceived(x => x.MakeParseOperation(testContext.MockBlock.Object), 1);

            mockBackgroundWorker.MockSubOperationScheduler
                .ShouldHaveReceived(x => x(testContext.MockParseOperation.Object), 1);

            result.Result.IsSkipped.ShouldBeTrue();
        }

        [TestCaseSource(nameof(BlockShouldBeManagedTestCases))]
        [TestCaseSource(nameof(BlockShouldNotBeManagedTestCases))]
        public void MakeCollectBlockOperation_OperationIsDisposed_OperationIsRecycled(CollectBlockTestCase testCase)
        {
            var testContext = new TestContext()
            {
                ManageOtherGrids     = testCase.ManageOtherGrids,
                AutoManageThisGrid   = testCase.AutoManageThisGrid,
                AutoManageOtherGrids = testCase.AutoManageOtherGrids,
                IsBlockSameGrid      = testCase.IsBlockSameGrid,
                IsBlockIgnored       = testCase.IsBlockIgnored,
                IsBlockManaged       = testCase.IsBlockManaged
            };
            var mockBackgroundWorker = new FakeBackgroundWorker();

            var result = testContext.Uut.MakeCollectBlockOperation(testContext.MockBlock.Object);

            result.ShouldRunToCompletion(mockBackgroundWorker);

            var mockBlockInvocations = testContext.MockBlock
                .Invocations.ToArray();

            var mockManagedBlockConfigManagerInvocations = testContext.MockManagedBlockConfigManager
                .Invocations.ToArray();

            var mockSubOperationSchedulerInvocations = mockBackgroundWorker.MockSubOperationScheduler
                .Invocations.ToArray();

            result.ShouldBeAssignableTo<IDisposable>();
            (result as IDisposable).Dispose();

            var secondMockBlock = new Mock<IMyTerminalBlock>();
            secondMockBlock
                .Setup(x => x.CubeGrid)
                .Returns(() => testContext.MockBlock.Object.CubeGrid);

            testContext.MockManagedBlockConfigManager
                .Invocations.Clear();

            mockBackgroundWorker.MockSubOperationScheduler
                .Invocations.Clear();

            testContext.Uut.MakeCollectBlockOperation(secondMockBlock.Object)
                .ShouldBeSameAs(result);

            result.ShouldRunToCompletion(mockBackgroundWorker);

            secondMockBlock.Invocations.Count.ShouldBe(mockBlockInvocations.Length);
            foreach (var i in Enumerable.Range(0, mockBlockInvocations.Length))
                secondMockBlock.Invocations[i].ShouldBe(mockBlockInvocations[i]);

            testContext.MockManagedBlockConfigManager.Invocations.Count.ShouldBe(mockManagedBlockConfigManagerInvocations.Length);
            foreach (var i in Enumerable.Range(0, mockManagedBlockConfigManagerInvocations.Length))
                testContext.MockManagedBlockConfigManager.Invocations[i].ShouldBe(mockManagedBlockConfigManagerInvocations[i]);

            mockBackgroundWorker.MockSubOperationScheduler.Invocations.Count.ShouldBe(mockSubOperationSchedulerInvocations.Length);
            foreach (var i in Enumerable.Range(0, mockSubOperationSchedulerInvocations.Length))
                mockBackgroundWorker.MockSubOperationScheduler.Invocations[i].ShouldBe(mockSubOperationSchedulerInvocations[i]);
        }

        #endregion MakeCollectBlockOperation() Tests

        #region OnCompleted() Tests

        [Test]
        public void OnCompleted_Always_DoesNothing()
        {
            var testContext = new TestContext();

            testContext.Uut.OnCompleted();

            testContext.MockManagedBlockConfigManager
                .Invocations.ShouldBeEmpty();

            testContext.MockManagedBlockSettingsProvider
                .Invocations.ShouldBeEmpty();

            testContext.MockManagerSettingsProvider
                .Invocations.ShouldBeEmpty();

            testContext.MockProgrammableBlock
                .Invocations.ShouldBeEmpty();
        }

        #endregion OnCompleted() Tests
    }
}
