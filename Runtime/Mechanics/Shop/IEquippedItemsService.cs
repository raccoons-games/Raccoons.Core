using System;

namespace Raccoons.Products
{
    public interface IEquippedItemsService
    {
        event Action<ShopItemCategoryAsset, BaseShopItemAsset> OnItemChanged;

        BaseShopItemAsset GetCurrentItem(ShopItemCategoryAsset category);
        bool IsCurrentItem(BaseShopItemAsset item);
        bool IsItemPurchased(string itemId);
        bool IsItemForAd(string itemId);
        void AddPurchasedItem(BaseShopItemAsset item);
        void SetCurrentItem(ShopItemCategoryAsset category, string itemId);
        void Initialize();
    }
}
