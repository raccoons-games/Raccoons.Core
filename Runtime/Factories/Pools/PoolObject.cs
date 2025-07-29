using System;
using UnityEngine;

namespace Raccoons.Factories.Pools
{
    public class PoolObject : RootInitializer, IInitializable, IDestroyHandler
    {
        public IPool OwningPool { get; internal set; }

        public event EventHandler<GameObject> OnDestroy;
        
        public void DestroySelf()
        {
            OnDestroy?.Invoke(this, gameObject);
            Return();
        }

        public virtual void Return()
        {
            OwningPool?.Return(this);
        }
    }
}