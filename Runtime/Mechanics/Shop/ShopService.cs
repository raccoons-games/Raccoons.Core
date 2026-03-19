using System;
using System.Collections.Generic;
using Raccoons.Scores;
using Raccoons.Storage;
using Zenject;

namespace Raccoons.Products
{
    public class ShopService : IShopService
    {
        private List<IShopPurchaseHandler> _purchaseHandlers;
        private IScoreBank _scoreBank;
        private IStorageChannel _storage;
        private ShopItemsRegistry _shopItemsRegistry;
        private IEquippedItemsService _equippedItemsService;

        public event Action<string> OnItemPurchased;
        public event Action<string> OnItemAdWatched;

        [Inject]
        private void Construct(IStorageChannel storage, IScoreBank scoreBank, ShopItemsRegistry shopItemsRegistry,
            List<IShopPurchaseHandler> purchaseHandlers, IEquippedItemsService equippedItemsService)
        {
            _equippedItemsService = equippedItemsService;
            _purchaseHandlers = purchaseHandlers;
            _shopItemsRegistry = shopItemsRegistry;
            _scoreBank = scoreBank;
            _storage = storage;
        }

        public void Initialize() { }

        public bool CanPurchaseAny()
        {
            foreach (BaseShopItemAsset item in _shopItemsRegistry.Items)
            {
                if (CanPurchase(item) && !item.AdPurchase)
                    return true;
            }

            return false;
        }

        public bool CanPurchase(string itemId)
        {
            var itemAsset = _shopItemsRegistry.GetShopItem<BaseShopItemAsset>(itemId);
            return CanPurchase(itemAsset);
        }

        private bool CanPurchase(BaseShopItemAsset itemAsset)
        {
            return _scoreBank.CanSpend(itemAsset.Price) && !_equippedItemsService.IsItemPurchased(itemAsset.Key);
        }

        public bool PurchaseItem(string itemId, bool forFree = false)
        {
            BaseShopItemAsset item = _shopItemsRegistry.GetShopItem<BaseShopItemAsset>(itemId);

            if (item.AdPurchase)
                return false;

            if (_scoreBank.CanSpend(item.Price) || forFree)
            {
                var purchaseHandler = GetTargetPurchaseHandler(item);
                if (!forFree)
                    _scoreBank.Spend(item.Price);

                purchaseHandler.HandlePurchase(item);
                OnItemPurchased?.Invoke(itemId);
                return true;
            }

            return false;
        }

        public bool AdPurchaseItem(string itemId)
        {
            BaseShopItemAsset item = _shopItemsRegistry.GetShopItem<BaseShopItemAsset>(itemId);

            if (!item.AdPurchase)
                return false;

            int watchedAds = GetWatchedRewards(itemId);

            //todo: add your ads provider here to purchase for RV
            // _adsProvider.RunRewarded(
            //     () =>
            //     {
            //         watchedAds++;
            //         SetWatchedRewards(itemId, watchedAds);
            //         OnItemAdWatched?.Invoke(itemId);
            //
            //         if (watchedAds >= item.AdViewsRequired)
            //         {
            //             GetTargetPurchaseHandler(item).HandlePurchase(item);
            //             OnItemPurchased?.Invoke(itemId);
            //         }
            //     });

            return true;
        }

        public int GetWatchedRewards(string itemId)
        {
            string key = GetAdProgressionKey(itemId);
            return _storage.Exists(key) ? _storage.GetInt(key) : 0;
        }

        public void SetWatchedRewards(string itemId, int rewardsCount)
        {
            _storage.SetInt(GetAdProgressionKey(itemId), rewardsCount);
        }

        public int GetRequiredRewards(string itemId)
        {
            return _shopItemsRegistry.GetShopItem<BaseShopItemAsset>(itemId).AdViewsRequired;
        }

        private string GetAdProgressionKey(string itemId) => itemId + "_rewards_watched";

        private IShopPurchaseHandler GetTargetPurchaseHandler(BaseShopItemAsset shopItemAsset)
        {
            foreach (IShopPurchaseHandler handler in _purchaseHandlers)
            {
                if (handler.CanProceed(shopItemAsset))
                    return handler;
            }

            return null;
        }
    }
}
