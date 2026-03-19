namespace Raccoons.Products
{
    public interface IShopPurchaseHandler
    {
        void HandlePurchase(BaseShopItemAsset item);
        bool CanProceed(BaseShopItemAsset itemAsset);
    }
}
