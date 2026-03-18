using Raccoons.Scores;
using UnityEngine;
using Zenject;

namespace Raccoons.Core.Runtime.Shop
{
    public class CanPurchaseItemTypeDot : BaseRedDotComponent
    {
        [SerializeField]
        private bool listenPurchases;

        [SerializeField]
        private ShopItemCategoryAsset itemCategory;

        private ShopItemsRegistry _shopItemsRegistry;
        private IShopService _shopService;
        private IScoreBank _scoreBank;

        [Inject]
        private void Construct(ShopItemsRegistry shopItemsRegistry, IShopService shopService, IScoreBank scoreBank)
        {
            _scoreBank = scoreBank;
            _shopService = shopService;
            _shopItemsRegistry = shopItemsRegistry;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (listenPurchases)
                _shopService.OnItemPurchased += ShopService_OnItemPurchased;

            _scoreBank.OnScoreChanged += ScoreBank_OnScoreChanged;
        }

        private void OnDisable()
        {
            if (listenPurchases)
                _shopService.OnItemPurchased -= ShopService_OnItemPurchased;

            _scoreBank.OnScoreChanged -= ScoreBank_OnScoreChanged;
        }

        private void ScoreBank_OnScoreChanged(object sender, ScoreChangeData e) => UpdateState();

        private void ShopService_OnItemPurchased(string key) => UpdateState();

        public override bool IsDotActive()
        {
            foreach (BaseShopItemAsset item in _shopItemsRegistry.GetAllItemsOfCategory(itemCategory))
            {
                if (_shopService.CanPurchase(item.Key) && !item.AdPurchase)
                    return true;
            }

            return false;
        }
    }
}
