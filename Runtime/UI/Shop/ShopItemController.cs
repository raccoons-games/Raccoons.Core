using System;
using System.Collections.Generic;
using Raccoons.Products;
using Raccoons.Scores;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Raccoons.UI.Shops
{
    public class ShopItemController : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup _adButtonCanvasGroup;

        [SerializeField]
        private BaseShopItemView shopItemView;

        [SerializeField]
        private Button button;

        [SerializeField]
        private List<GameObject> selectedState;

        [SerializeField]
        private List<GameObject> purchasedState;

        [SerializeField]
        private List<GameObject> noEnoughMoneyState;

        [SerializeField]
        private List<GameObject> adToWatchState;

        [SerializeField]
        private GameObject nonPurchasedState;

        [SerializeField]
        private List<GameObject> redDot;

        private const float DisabledAlpha = 0.5f;

        private IShopService _shopService;
        private IEquippedItemsService _equippedItemsService;
        private IScoreBank _scoreBank;

        private bool _isInteractionBlocked;

        public BaseShopItemAsset ShopItemAsset { get; private set; }
        public event Action<ShopItemController> OnClicked;

        [Inject]
        private void Construct(IShopService shopService, IEquippedItemsService equippedItemsService, IScoreBank scoreBank)
        {
            _scoreBank = scoreBank;
            _equippedItemsService = equippedItemsService;
            _shopService = shopService;
        }

        public void Initialize(BaseShopItemAsset shopItemAsset)
        {
            ShopItemAsset = shopItemAsset;
            shopItemView.Initialize(ShopItemAsset);
            shopItemView.UpdateView();
            UpdateState();
        }

        public void UpdateState()
        {
            bool isPurchased = _equippedItemsService.IsItemPurchased(ShopItemAsset.Key);
            bool isSelected = _equippedItemsService.IsCurrentItem(ShopItemAsset);
            bool isAdToWatch = _equippedItemsService.IsItemForAd(ShopItemAsset.Key);
            bool canPurchase = _shopService.CanPurchase(ShopItemAsset.Key);

            bool isAdItem = isAdToWatch && !isSelected && !isPurchased;

            _isInteractionBlocked = !canPurchase && !isSelected && !isPurchased;

            UpdateDisabledVisual(_isInteractionBlocked);

            purchasedState.ForEach(item => item.SetActive(false));
            selectedState.ForEach(item => item.SetActive(false));
            noEnoughMoneyState.ForEach(item => item.SetActive(false));
            adToWatchState.ForEach(item => item.SetActive(false));
            nonPurchasedState.SetActive(false);

            if (isSelected)
                selectedState.ForEach(item => item.SetActive(true));
            else if (isPurchased)
                purchasedState.ForEach(item => item.SetActive(true));
            else if (isAdItem)
                adToWatchState.ForEach(item => item.SetActive(true));
            else if (!canPurchase)
                noEnoughMoneyState.ForEach(item => item.SetActive(true));
            else
                nonPurchasedState.SetActive(true);

            redDot.ForEach(item => item.SetActive(canPurchase && !isPurchased && !isAdToWatch));
        }

        private void UpdateDisabledVisual(bool isDisabled)
        {
            _adButtonCanvasGroup.alpha = isDisabled ? DisabledAlpha : 1f;
        }

        private void Awake() => button.onClick.AddListener(Button_OnClicked);

        private void OnEnable()
        {
            _shopService.OnItemPurchased += ShopService_OnItemPurchased;
            _shopService.OnItemAdWatched += ShopService_OnRewardedWatched;
            _scoreBank.OnScoreChanged += ScoreBank_OnScoreChanged;
            UpdateState();
        }

        private void OnDisable()
        {
            _shopService.OnItemPurchased -= ShopService_OnItemPurchased;
            _shopService.OnItemAdWatched -= ShopService_OnRewardedWatched;
            _scoreBank.OnScoreChanged -= ScoreBank_OnScoreChanged;
        }

        private void OnDestroy() => button.onClick.RemoveListener(Button_OnClicked);

        private void ScoreBank_OnScoreChanged(object sender, ScoreChangeData e) => UpdateState();

        private void ShopService_OnItemPurchased(string itemId) => UpdateState();

        private void ShopService_OnRewardedWatched(string itemId) => shopItemView.UpdateView();

        private void Button_OnClicked()
        {
            if (_isInteractionBlocked)
                return;

            OnClicked?.Invoke(this);
        }

        public void Show() => gameObject.SetActive(true);

        public void Hide() => gameObject.SetActive(false);

        public void Select()
        {
            selectedState.ForEach(item => item.SetActive(true));
            purchasedState.ForEach(item => item.SetActive(false));
        }

        public void Unselect()
        {
            selectedState.ForEach(item => item.SetActive(false));
            bool isPurchased = _equippedItemsService.IsItemPurchased(ShopItemAsset.Key);
            if (isPurchased)
                purchasedState.ForEach(item => item.SetActive(true));
        }
    }
}
