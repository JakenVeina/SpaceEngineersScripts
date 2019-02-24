using System;
using System.Collections.Generic;
using System.Linq;

using Moq;
using NUnit.Framework;
using Shouldly;

using static IngameScript.Program;

namespace Mixins.Test
{
    [TestFixture]
    public class ObjectPoolTests
    {
        #region Test Context

        private static Mock<IDisposable> MakeFakeDisposable(Action onDisposed)
        {
            var mockObject = new Mock<IDisposable>();

            mockObject
                .Setup(x => x.Dispose())
                .Callback(onDisposed);

            return mockObject;
        }

        #endregion Test Context

        #region Count Tests

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void Count_Always_ReturnsPooledObjectCount(int count)
        {
            var uut = new ObjectPool<IDisposable>(onFinished => MakeFakeDisposable(onFinished).Object);

            var objects = Enumerable.Repeat(0, count)
                .Select(x => uut.Get())
                .ToArray();

            foreach (var @object in objects)
                @object.Dispose();

            uut.Count.ShouldBe(count);
        }

        #endregion Count Tests

        #region Get() Tests

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void Get_PoolIsEmpty_CreatesNewInstance(int existingObjectCount)
        {
            var mockDisposables = new List<Mock<IDisposable>>();

            var mockConstructor = new Mock<PooledObjectConstructor<IDisposable>>();
            mockConstructor
                .Setup(x => x(It.IsAny<Action>()))
                .Returns<Action>(onFinished =>
                {
                    var mockDisposable = MakeFakeDisposable(onFinished);

                    mockDisposables.Add(mockDisposable);

                    return mockDisposable.Object;
                });

            var uut = new ObjectPool<IDisposable>(mockConstructor.Object);

            var existingObjects = Enumerable.Repeat(0, existingObjectCount)
                .Select(_ => uut.Get())
                .ToArray();

            foreach (var existingObject in existingObjects)
                existingObject.Dispose();

            foreach (var _ in Enumerable.Repeat(0, existingObjectCount))
                uut.Get();

            mockConstructor.Invocations.Clear();

            var result = uut.Get();

            result.ShouldBeSameAs(mockDisposables.Last().Object);

            mockConstructor.ShouldHaveReceived(x => x(It.IsAny<Action>()), 1);
        }

        [TestCase(1)]
        [TestCase(10)]
        public void Get_PoolIsNotEmpty_ReturnsExistingInstance(int existingObjectCount)
        {
            var mockDisposables = new List<Mock<IDisposable>>();

            var mockConstructor = new Mock<PooledObjectConstructor<IDisposable>>();
            mockConstructor
                .Setup(x => x(It.IsAny<Action>()))
                .Returns<Action>(onFinished =>
                {
                    var mockDisposable = MakeFakeDisposable(onFinished);

                    mockDisposables.Add(mockDisposable);

                    return mockDisposable.Object;
                });

            var uut = new ObjectPool<IDisposable>(mockConstructor.Object);

            var existingObjects = Enumerable.Repeat(0, existingObjectCount)
                .Select(_ => uut.Get())
                .ToArray();

            foreach (var existingObject in existingObjects)
                existingObject.Dispose();

            mockConstructor.Invocations.Clear();

            var result = uut.Get();

            result.ShouldBeSameAs(mockDisposables.First().Object);

            mockConstructor.ShouldNotHaveReceived(x => x(It.IsAny<Action>()));
        }

        #endregion Get() Tests

        #region onFinished Tests

        [TestCase(1)]
        [TestCase(10)]
        public void OnFinished_Always_ReturnsObjectToPool(int count)
        {
            var uut = new ObjectPool<IDisposable>(onFinished => MakeFakeDisposable(onFinished).Object);

            var objects = Enumerable.Repeat(0, count)
                .Select(x => uut.Get())
                .ToArray();

            foreach (var @object in objects)
                @object.Dispose();

            var returnedObjects = Enumerable.Repeat(0, count)
                .Select(x => uut.Get())
                .ToArray();

            returnedObjects.ShouldBe(objects);
        }

        #endregion onFinished Tests
    }
}
