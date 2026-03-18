using UnityEngine;

namespace Raccoons.Core.Runtime.Shop
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
