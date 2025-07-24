using UnityEngine;

namespace Raccoons.Factories
{
    public class InstantiateFactory : BaseFactory
    {
        protected override GameObject CreateInternal()
        {
            return Instantiate(prefab);
        }
    }
}