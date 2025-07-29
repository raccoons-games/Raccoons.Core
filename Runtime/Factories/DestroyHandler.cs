using System;
using UnityEngine;

namespace Raccoons.Factories
{
    public class DestroyHandler : MonoBehaviour, IDestroyHandler
    {
        public event EventHandler<GameObject> OnDestroy;

        public void DestroySelf()
        {
            OnDestroy?.Invoke(this, gameObject);
            Destroy(gameObject);
        }
    }
}