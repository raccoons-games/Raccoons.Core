using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Raccoons.Core.Runtime.Shop
{
    public class ConfirmAdPurchasePopup : MonoBehaviour
    {
        [SerializeField]
        private Button purchaseButton;

        [SerializeField]
        private Button closeButton;

        [SerializeField]
        private BaseShopItemView shopItemView;

        private BaseShopItemAsset _shopItemAsset;
        private IShopService _shopService;
        private bool _isInteractionBlocked;

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
            UpdateAdAvailability();
        }

        private void ProcessPurchase()
        {
            if (_isInteractionBlocked)
                return;

            bool result = _shopService.AdPurchaseItem(_shopItemAsset.Key);
            if (result)
                Hide();
        }

        public void Hide() => gameObject.SetActive(false);

        private void UpdateAdAvailability()
        {
            // _isInteractionBlocked = !_adsProvider.IsRewardedReady();
            _isInteractionBlocked = false;
        }
    }
}
