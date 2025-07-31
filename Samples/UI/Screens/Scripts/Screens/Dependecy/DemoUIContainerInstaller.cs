using UnityEngine;
using Zenject;

namespace Raccoons.UI.Samples.Screens
{
    public class DemoUIContainerInstaller : MonoInstaller
    {
        [SerializeField]
        private DemoScreensManager _demoScreensManager;
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<DemoScreensManager>().FromInstance(_demoScreensManager).AsSingle();
        }
    }
}