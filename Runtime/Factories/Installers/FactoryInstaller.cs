using Raccoons.Identifiers.Guids;
using UnityEngine;
using Zenject;

namespace Raccoons.Factories.Installers
{
    public class FactoryInstaller : MonoInstaller
    {
        [SerializeField] private BaseFactory factory;
        [SerializeField] private GuidAsset guid;

        public override void InstallBindings()
        {
            Container.BindInstance(factory).WithId(guid).AsSingle();
        }
    }
}