using UnityEngine;

namespace Raccoons.Factories
{
    public abstract class BaseFactory : MonoBehaviour, IFactory
    {
        [SerializeField]
        protected GameObject prefab;

        private IDependenciesProvider _dependenciesProvider;
        
        public GameObject Prefab => prefab;
        
        private void Awake()
        {
            _dependenciesProvider = GetComponent<IDependenciesProvider>();
        }

        public virtual GameObject Create()
        {
            var instance = CreateInternal();
            var initializer = instance.GetComponent<RootInitializer>();
            if (initializer != null)
            {
                initializer.Initialize(_dependenciesProvider);
            }

            return instance;
        }

        protected abstract GameObject CreateInternal();
    }
}