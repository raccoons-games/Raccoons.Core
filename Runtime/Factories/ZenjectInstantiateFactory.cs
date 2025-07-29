using UnityEngine;
using Zenject;

namespace Raccoons.Factories
{
    public class ZenjectInstantiateFactory : BaseFactory
    {
        private DiContainer _container;

        [Inject]
        public void Construct(DiContainer container)
        {
            _container = container;
        }
        
        protected override GameObject CreateInternal()
        {
            return _container.InstantiatePrefab(prefab);
        }
    }
}