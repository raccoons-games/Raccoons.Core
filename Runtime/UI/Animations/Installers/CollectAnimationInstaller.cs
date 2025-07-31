using Raccoons.Factories.Installers;
using Raccoons.Factories.Pools;
using UnityEngine;
using Zenject;

namespace Raccoons.UI.Animations.Installers
{
    [RequireComponent(typeof(Pool))]
    public class CollectAnimationInstaller : FactoryInstaller
    {
        [SerializeField] private CollectAnimationSystem collectAnimationSystem;

        public override void InstallBindings()
        {
            base.InstallBindings();
            Container.Bind<CollectAnimationSystem>().FromInstance(collectAnimationSystem).AsSingle();
        }
    }
} 