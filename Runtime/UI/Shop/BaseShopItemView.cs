using Raccoons.Products;
using UnityEngine;

namespace Raccoons.UI.Shops
{
    public abstract class BaseShopItemView : MonoBehaviour
    {
        protected BaseShopItemAsset ShopItemAsset;

        public virtual void Initialize(BaseShopItemAsset shopItemAsset)
        {
            ShopItemAsset = shopItemAsset;
        }

        public abstract void UpdateView();
    }
}
