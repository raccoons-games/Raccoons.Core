using System;
using Raccoons.Infrastructure;

namespace Raccoons.Products
{
    public interface IShopService: IGlobalService
    {
        event Action<string> OnItemPurchased;
        event Action<string> OnItemAdWatched;

        bool PurchaseItem(string itemId, bool forFree = false);
        bool AdPurchaseItem(string itemId);
        bool CanPurchase(string itemId);
        bool CanPurchaseAny();
        int GetWatchedRewards(string itemId);
        void SetWatchedRewards(string itemId, int rewardsCount);
        int GetRequiredRewards(string itemId);
    }
}
