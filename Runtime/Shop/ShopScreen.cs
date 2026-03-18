using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Raccoons.Core.Runtime.Shop
{
    public class ShopScreen : MonoBehaviour
    {
        [SerializeField]
        private List<ShopTabItemsController> shopTabItemsControllers;

        [SerializeField]
        private ConfirmPurchasePopup confirmPurchasePopup;

        [SerializeField]
        private ConfirmAdPurchasePopup confirmAdPurchasePopup;

        [SerializeField]
        private Button closeButton;

        private void Start()
        {
            foreach (ShopTabItemsController itemsController in shopTabItemsControllers)
            {
                itemsController.OnPurchaseRequested += ItemsController_OnPurchaseRequested;
            }

            closeButton.onClick.AddListener(Hide);
        }

        private void OnDestroy()
        {
            closeButton.onClick.RemoveListener(Hide);
        }

        private void ItemsController_OnPurchaseRequested(BaseShopItemAsset shopItemAsset)
        {
            if (shopItemAsset.AdPurchase)
                confirmAdPurchasePopup.Show(shopItemAsset);
            else
                confirmPurchasePopup.Show(shopItemAsset);
        }

        public void Show() => gameObject.SetActive(true);

        public void Hide() => gameObject.SetActive(false);
    }
}
