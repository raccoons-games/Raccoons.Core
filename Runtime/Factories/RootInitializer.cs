using System;
using System.Collections.Generic;
using UnityEngine;

namespace Raccoons.Factories
{
    public class RootInitializer : MonoBehaviour, IDependenciesProvider
    {
        private IDependenciesProvider _globalDependenciesProvider;
        private Dictionary<Type, object> _localDependencies = new();
        public virtual void Initialize(IDependenciesProvider dependenciesProvider)
        {
            CacheLocalDependencies(_localDependencies);
            _globalDependenciesProvider = dependenciesProvider;
            // override in child if needed
            var children = GetComponentsInChildren<IInitializable>();
            foreach (var initializable in children)
            {
                initializable.Initialize(dependenciesProvider);
            }
        }

        public virtual void CacheLocalDependencies(Dictionary<Type, object> internalDependencies)
        {
            
        }

        public T Get<T>()
        {
            if (_localDependencies.TryGetValue(typeof(T), out var result) && result != null)
            {
                return (T)result;
            }

            return _globalDependenciesProvider.Get<T>();
        }
    }
}