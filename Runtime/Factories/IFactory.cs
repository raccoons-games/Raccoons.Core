using UnityEngine;

namespace Raccoons.Factories
{
    public interface IFactory
    {
        GameObject Create();
    }
}