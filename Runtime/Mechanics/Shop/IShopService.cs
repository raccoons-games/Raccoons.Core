using System;

namespace Raccoons.Products
{
    public interface IShopService
    {
        event Action<string> OnItemPurchased;
        event Action<string> OnItemAdWatched;

        bool PurchaseItem(string itemId, bool forFree = false);
        bool AdPurchaseItem(string itemId);
        bool CanPurchase(string itemId);
        bool CanPurchaseAny();
        void Initialize();
        int GetWatchedRewards(string itemId);
        void SetWatchedRewards(string itemId, int rewardsCount);
        int GetRequiredRewards(string itemId);
    }
}
