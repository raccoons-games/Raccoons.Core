using System;
using System.Collections.Generic;
using System.Linq;
using Raccoons.Products;
using UnityEngine;
using Zenject;

namespace Raccoons.UI.Shops
{
    public class ShopTabItemsController : MonoBehaviour
    {
        [SerializeField]
        private GameObject itemsContainer;

        [SerializeField]
        private ShopItemController itemPrefab;

        [SerializeField]
        private ShopItemCategoryAsset itemCategory;

        private List<ShopItemController> _items = new();

        private IEquippedItemsService _equippedItemsService;
        private ShopItemsRegistry _shopItemsRegistry;
        private DiContainer _diContainer;

        private ShopItemController _currentSelectedItem;
        public ShopItemController CurrentSelectedItem => _currentSelectedItem;

        public event Action<BaseShopItemAsset> OnPurchaseRequested;

        [Inject]
        private void Construct(ShopItemsRegistry shopItemsRegistry, IEquippedItemsService equippedItemsService,
            DiContainer diContainer)
        {
            _equippedItemsService = equippedItemsService;
            _shopItemsRegistry = shopItemsRegistry;
            _diContainer = diContainer;
        }

        private void Start()
        {
            foreach (BaseShopItemAsset item in _shopItemsRegistry.GetAllItemsOfCategory(itemCategory))
            {
                CreateItem(item);
            }

            BaseShopItemAsset current = _equippedItemsService.GetCurrentItem(itemCategory);
            _currentSelectedItem = _items.FirstOrDefault(item => item.ShopItemAsset == current);
            UpdateSelectState(_currentSelectedItem);

            _equippedItemsService.OnItemChanged += OnEquippedItemChanged;
        }

        private void OnDestroy()
        {
            _equippedItemsService.OnItemChanged -= OnEquippedItemChanged;
        }

        private void OnEquippedItemChanged(ShopItemCategoryAsset category, BaseShopItemAsset item)
        {
            if (category == itemCategory)
                UpdateSelectState(item);
        }

        private void CreateItem(BaseShopItemAsset item)
        {
            ShopItemController instance = _diContainer.InstantiatePrefabForComponent<ShopItemController>(
                itemPrefab.gameObject, itemsContainer.transform);
            instance.Initialize(item);
            instance.gameObject.SetActive(true);
            instance.OnClicked += ShopItem_OnClicked;
            _items.Add(instance);
        }

        private void ShopItem_OnClicked(ShopItemController item)
        {
            if (_equippedItemsService.IsItemPurchased(item.ShopItemAsset.Key))
                SelectItem(item);
            else
                OnPurchaseRequested?.Invoke(item.ShopItemAsset);
        }

        private void SelectItem(ShopItemController item)
        {
            _equippedItemsService.SetCurrentItem(itemCategory, item.ShopItemAsset.Key);
            UpdateSelectState(item);
        }

        private void UpdateSelectState(ShopItemController selectedItem)
        {
            _currentSelectedItem = selectedItem;
            if (selectedItem != null)
                selectedItem.Select();

            foreach (ShopItemController itemController in _items)
            {
                if (itemController != selectedItem)
                    itemController.Unselect();
            }
        }

        private void UpdateSelectState(BaseShopItemAsset selectedItem)
        {
            ShopItemController match = _items.FirstOrDefault(item => item.ShopItemAsset == selectedItem);
            if (match != null)
                UpdateSelectState(match);
        }
    }
}
