using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Raccoons.Factories.Pools
{
    public class Pool : BaseFactory, IPool
    {
        [SerializeField] private int initialSize = 10;
        
        private readonly Queue<PoolObject> _poolQueue = new();
        private IDependenciesProvider _dependenciesProvider;

        protected virtual void Awake()
        {
            for (int i = 0; i < initialSize; i++)
            {
                AddNewInstanceToPool();
            }

            _dependenciesProvider = GetComponent<IDependenciesProvider>();
        }

        private void AddNewInstanceToPool()
        {
            var instance = Instantiate(prefab, transform);
            instance.SetActive(false);
            
            if (instance.TryGetComponent<PoolObject>(out var poolObj)) { }
            else
            {
                if (instance.TryGetComponent<IDestroyHandler>(out var destroyHandler))
                {
                    Destroy(destroyHandler as Component);
                }
                poolObj = instance.AddComponent<PoolObject>();
            }
            _poolQueue.Enqueue(poolObj);
            poolObj.OwningPool = this;
        }

        protected override GameObject CreateInternal()
        {
            PoolObject obj;
            if (!_poolQueue.Any())
            {
                AddNewInstanceToPool();
            }
            obj = _poolQueue.Dequeue();
            obj.gameObject.SetActive(true);
            return obj.gameObject;
        }
        
        public virtual void Return(PoolObject poolObject)
        {
            poolObject.gameObject.SetActive(false);
            _poolQueue.Enqueue(poolObject);
        }

    }

}