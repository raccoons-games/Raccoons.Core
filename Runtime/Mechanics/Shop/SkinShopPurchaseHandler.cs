using Zenject;

namespace Raccoons.Products
{
    public class SkinShopPurchaseHandler : IShopPurchaseHandler
    {
        private IEquippedItemsService _equippedItemsService;

        [Inject]
        private void Construct(IEquippedItemsService equippedItemsService)
        {
            _equippedItemsService = equippedItemsService;
        }

        public bool CanProceed(BaseShopItemAsset itemAsset) => itemAsset is SpriteShopItemAsset;

        public void HandlePurchase(BaseShopItemAsset item) => _equippedItemsService.AddPurchasedItem(item);
    }
}
