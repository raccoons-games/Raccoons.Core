using Raccoons.Products;
using UnityEngine;
using Zenject;

namespace Raccoons.UI.Shops
{
    public class EquippedItemViewController : MonoBehaviour
    {
        [SerializeField]
        private ShopItemCategoryAsset category;

        [SerializeField]
        private BaseShopItemView itemView;

        private IEquippedItemsService _equippedItemsService;

        [Inject]
        private void Construct(IEquippedItemsService equippedItemsService)
        {
            _equippedItemsService = equippedItemsService;
        }

        private void OnEnable()
        {
            _equippedItemsService.OnItemChanged += OnItemChanged;
            UpdateView(_equippedItemsService.GetCurrentItem(category));
        }

        private void OnDisable()
        {
            _equippedItemsService.OnItemChanged -= OnItemChanged;
        }

        private void OnItemChanged(ShopItemCategoryAsset changedCategory, BaseShopItemAsset item)
        {
            if (changedCategory == category)
                UpdateView(item);
        }

        private void UpdateView(BaseShopItemAsset item)
        {
            if (item == null)
                return;
            itemView.Initialize(item);
            itemView.UpdateView();
        }
    }
}
