using UnityEngine;

namespace Raccoons.Factories
{
    public class RootInitializer : MonoBehaviour
    {
        public virtual void Initialize(IDependenciesProvider dependenciesProvider)
        {
            // override in child if needed
            var children = GetComponentsInChildren<IInitializable>();
            foreach (var initializable in children)
            {
                initializable.Initialize(dependenciesProvider);
            }
        }
    }
}