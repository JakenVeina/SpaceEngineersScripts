using System;
using System.Collections.Generic;

namespace IngameScript
{
    public partial class Program
    {
        public delegate T PooledObjectConstructor<T>(Action onFinished) where T : class;

        public interface IObjectPool<T> where T : class
        {
            int Count { get; }

            T Get();
        }

        public partial class ObjectPool<T> : IObjectPool<T> where T : class
        {
            public ObjectPool(PooledObjectConstructor<T> constructor)
            {
                _constructor = constructor;
            }

            public int Count
                => _pool.Count;

            public T Get()
            {
                PooledObject pooledObject;
                if(!_pool.TryDequeue(out pooledObject))
                {
                    pooledObject = new PooledObject()
                    {
                        Pool = _pool,
                    };

                    pooledObject.Instance = _constructor.Invoke(pooledObject.OnFinished);
                }

                return pooledObject.Instance;
            }

            private readonly Queue<PooledObject> _pool
                = new Queue<PooledObject>();

            private readonly PooledObjectConstructor<T> _constructor;

            private class PooledObject
            {
                public Queue<PooledObject> Pool;

                public T Instance;

                public void OnFinished()
                    => Pool.Enqueue(this);
            }
        }
    }
}
