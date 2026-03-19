using Raccoons.Products;
using Zenject;

namespace Raccoons.UI.Shops
{
    public class ShopButtonRedDot : BaseRedDotComponent
    {
        private IShopService _shopService;

        [Inject]
        private void Construct(IShopService shopService)
        {
            _shopService = shopService;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _shopService.OnItemPurchased += ShopService_OnItemPurchased;
        }

        private void OnDisable()
        {
            _shopService.OnItemPurchased -= ShopService_OnItemPurchased;
        }

        private void ShopService_OnItemPurchased(string _) => UpdateState();

        public override bool IsDotActive() => _shopService.CanPurchaseAny();
    }
}
