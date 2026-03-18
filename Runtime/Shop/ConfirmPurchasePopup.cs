using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Raccoons.Core.Runtime.Shop
{
    public class ConfirmPurchasePopup : MonoBehaviour
    {
        [SerializeField]
        private Button purchaseButton;

        [SerializeField]
        private Button closeButton;

        [SerializeField]
        private BaseShopItemView shopItemView;

        private BaseShopItemAsset _shopItemAsset;
        private IShopService _shopService;

        [Inject]
        private void Construct(IShopService shopService)
        {
            _shopService = shopService;
        }

        private void Awake()
        {
            closeButton.onClick.AddListener(Hide);
            purchaseButton.onClick.AddListener(ProcessPurchase);
        }

        public void Show(BaseShopItemAsset shopItemAsset)
        {
            shopItemView.Initialize(shopItemAsset);
            shopItemView.UpdateView();
            gameObject.SetActive(true);
            _shopItemAsset = shopItemAsset;
        }

        private void ProcessPurchase()
        {
            bool result = _shopService.PurchaseItem(_shopItemAsset.Key);
            if (result)
                Hide();
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
