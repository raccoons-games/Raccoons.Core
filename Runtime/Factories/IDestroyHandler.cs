using System;
using UnityEngine;

namespace Raccoons.Factories
{
    public interface IDestroyHandler
    {
        event EventHandler<GameObject> OnDestroy;
        void DestroySelf();
    }
}