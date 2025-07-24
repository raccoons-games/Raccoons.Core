using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Raccoons.Factories
{
    public enum ZenjectContainerTargeting { Injected, SceneContext }
    public class ZenjectDependenciesProvider : MonoBehaviour, IDependenciesProvider
    {
        [SerializeField] private ZenjectContainerTargeting containerTargeting = ZenjectContainerTargeting.Injected;
        [SerializeField] private bool cacheDependencies = true;
        
        private DiContainer _container;
        private readonly Dictionary<Type, object> _cachedDependencies = new Dictionary<Type, object>();

        public DiContainer Container => _container;

        private void Awake()
        {
            if (containerTargeting == ZenjectContainerTargeting.SceneContext)
            {
                SceneManager.sceneLoaded += SceneManagerOnSceneLoaded;
            }
        }

        private void SceneManagerOnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            _container = null;
            _cachedDependencies.Clear();
            EnsureContainer();
        }

        [Inject]
        public void Construct(DiContainer container)
        {
            if (containerTargeting == ZenjectContainerTargeting.Injected)
            {
                _container = container;
            }
        }

        public T Get<T>()
        {
            EnsureContainer();
            if (_cachedDependencies.TryGetValue(typeof(T), out var result) && result != null)
            {
                return (T)result;
            }
            result = Container.Resolve<T>();
            if (cacheDependencies)
            {
                _cachedDependencies.Add(typeof(T), result);
            }

            return (T)result;
        }

        private void EnsureContainer()
        {
            if (containerTargeting == ZenjectContainerTargeting.SceneContext && Container == null)
            {
                _container = FindObjectOfType<SceneContext>().Container;
            }
        }
    }
}