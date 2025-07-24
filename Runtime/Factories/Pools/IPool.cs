using UnityEngine;

namespace Raccoons.Factories.Pools
{
    public interface IPool
    {
        GameObject Create();
        void Return(PoolObject poolObject);
    }
}