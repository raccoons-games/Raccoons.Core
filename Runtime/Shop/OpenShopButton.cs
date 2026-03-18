using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Raccoons.Core.Runtime.Shop
{
    public class OpenShopButton : MonoBehaviour
    {
        [SerializeField]
        private Button button;

        private ShopScreenStateController _shopScreenState;

        [Inject]
        private void Construct(ShopScreenStateController shopScreenState)
        {
            _shopScreenState = shopScreenState;
        }

        private void Awake() => button.onClick.AddListener(Button_OnClicked);

        private void OnDestroy() => button.onClick.RemoveListener(Button_OnClicked);

        private void Button_OnClicked() => _shopScreenState.Show();
    }
}
