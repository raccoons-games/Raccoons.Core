namespace Raccoons.Core.Runtime.Shop
{
    public interface IShopPurchaseHandler
    {
        void HandlePurchase(BaseShopItemAsset item);
        bool CanProceed(BaseShopItemAsset itemAsset);
    }
}
