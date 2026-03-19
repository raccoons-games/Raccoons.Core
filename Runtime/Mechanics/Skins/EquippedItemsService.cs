using System;
using System.Collections.Generic;
using Raccoons.Serialization;
using Raccoons.Storage;
using Zenject;

namespace Raccoons.Products
{
    public class EquippedItemsService : IEquippedItemsService
    {
        private const string SaveKey = "equipped_items_save";

        private readonly Dictionary<string, BaseShopItemAsset> _currentItems = new();
        private EquippedItemsSaveData _saveData;

        private IStorageChannel _storage;
        private ISerializer _serializer;
        private ShopItemsRegistry _shopItemsRegistry;

        public event Action<ShopItemCategoryAsset, BaseShopItemAsset> OnItemChanged;

        [Inject]
        private void Construct(IStorageChannel storage, ISerializer serializer, ShopItemsRegistry shopItemsRegistry)
        {
            _shopItemsRegistry = shopItemsRegistry;
            _serializer = serializer;
            _storage = storage;
            Initialize();
        }

        public void Initialize()
        {
            _saveData = LoadSaveData();
            ValidateSaveData(_saveData);

            foreach (var entry in _saveData.CurrentItems)
            {
                var item = _shopItemsRegistry.GetShopItem<BaseShopItemAsset>(entry.Value);
                if (item != null)
                    _currentItems[entry.Key] = item;
            }
        }

        private void ValidateSaveData(EquippedItemsSaveData saveData)
        {
            if (saveData.PurchasedItems == null)
                saveData.PurchasedItems = new List<string>();
            if (saveData.CurrentItems == null)
                saveData.CurrentItems = new Dictionary<string, string>();

            foreach (BaseShopItemAsset item in _shopItemsRegistry.Items)
            {
                if (item.IsFree && !saveData.PurchasedItems.Contains(item.Key))
                    saveData.PurchasedItems.Add(item.Key);

                string categoryKey = item.Category.Key;
                if (!saveData.CurrentItems.ContainsKey(categoryKey) && item.IsFree)
                    saveData.CurrentItems[categoryKey] = item.Key;
            }

            SaveData();
        }

        public BaseShopItemAsset GetCurrentItem(ShopItemCategoryAsset category)
        {
            _currentItems.TryGetValue(category.Key, out BaseShopItemAsset item);
            return item;
        }

        public bool IsCurrentItem(BaseShopItemAsset item)
        {
            foreach (BaseShopItemAsset current in _currentItems.Values)
            {
                if (current == item)
                    return true;
            }
            return false;
        }

        public void SetCurrentItem(ShopItemCategoryAsset category, string itemId)
        {
            var item = _shopItemsRegistry.GetShopItem<BaseShopItemAsset>(itemId);
            _currentItems[category.Key] = item;
            _saveData.CurrentItems[category.Key] = itemId;
            SaveData();
            OnItemChanged?.Invoke(category, item);
        }

        public bool IsItemPurchased(string itemId)
            => _saveData.PurchasedItems.Contains(itemId);

        public bool IsItemForAd(string itemId)
        {
            var item = _shopItemsRegistry.GetShopItem<BaseShopItemAsset>(itemId);
            return item.AdPurchase;
        }

        public void AddPurchasedItem(BaseShopItemAsset item)
        {
            _saveData.AddPurchasedItem(item.Key);
            SaveData();
        }

        private EquippedItemsSaveData LoadSaveData()
        {
            if (!_storage.Exists(SaveKey))
                return new EquippedItemsSaveData();

            string json = _storage.GetString(SaveKey);
            return _serializer.Deserialize<EquippedItemsSaveData>(json) ?? new EquippedItemsSaveData();
        }

        private void SaveData()
        {
            _storage.SetString(SaveKey, _serializer.Serialize(_saveData));
        }
    }
}
