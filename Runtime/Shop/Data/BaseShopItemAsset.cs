using Raccoons.Identifiers.Guids;
using UnityEngine;

namespace Raccoons.Core.Runtime.Shop
{
    public abstract class BaseShopItemAsset : GuidAsset
    {
        [field: SerializeField]
        public bool IsFree { get; private set; }

        [field: SerializeField]
        public int Price { get; private set; }

        [field: SerializeField]
        public bool AdPurchase { get; private set; }

        [field: SerializeField]
        public int AdViewsRequired { get; private set; }

        [SerializeField]
        private ShopItemCategoryAsset category;

        public ShopItemCategoryAsset Category => category;
    }
}
