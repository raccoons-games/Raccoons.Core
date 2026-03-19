using Raccoons.UI.Screens;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Raccoons.UI.Shops
{
    public class OpenShopButton : MonoBehaviour
    {
        [SerializeField]
        private Button button;

        private BaseScreenManager _basescreenManager;

        [Inject]
        private void Construct(BaseScreenManager basescreenManager)
        {
            _basescreenManager = basescreenManager;
        }

        private void Awake() => button.onClick.AddListener(Button_OnClicked);

        private void OnDestroy() => button.onClick.RemoveListener(Button_OnClicked);

        private void Button_OnClicked() => _basescreenManager.ShowScreen<ShopScreen>();
    }
}
