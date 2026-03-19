using System.Collections.Generic;
using Raccoons.Products;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Raccoons.UI.Shops
{
    public class ShopItemView : BaseShopItemView
    {
        [Header("Price")]
        [SerializeField] private List<TMP_Text> priceText;

        [Header("Ad Progress")]
        [SerializeField] private TMP_Text adProgressText;

        [Header("Skin Preview")]
        [SerializeField] private Image skinImage;

        private IShopService _shopService;
        private SpriteShopItemAsset _spriteAsset;

        [Inject]
        private void Construct(IShopService shopService)
        {
            _shopService = shopService;
        }

        public override void Initialize(BaseShopItemAsset shopItemAsset)
        {
            base.Initialize(shopItemAsset);
            _spriteAsset = shopItemAsset as SpriteShopItemAsset;
        }

        public override void UpdateView()
        {
            if (priceText != null)
                priceText.ForEach(item => item.text = ShopItemAsset.Price.ToString());

            if (adProgressText != null)
            {
                string itemId = ShopItemAsset.Key;
                int watchedAds = _shopService.GetWatchedRewards(itemId);
                int requiredAds = _shopService.GetRequiredRewards(itemId);
                adProgressText.text = watchedAds + "/" + requiredAds;
            }

            if (skinImage != null && _spriteAsset != null)
                skinImage.sprite = _spriteAsset.Background;
        }
    }
}
