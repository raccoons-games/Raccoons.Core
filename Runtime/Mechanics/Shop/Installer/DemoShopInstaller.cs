using Raccoons.Scores;
using Raccoons.Scores.Banks;
using Raccoons.Scores.Storages;
using Raccoons.Serialization;
using Raccoons.Serialization.Json;
using Raccoons.Storage;
using Raccoons.Storage.Cryptography;
using Raccoons.Storage.Cryptography.Aes;
using Raccoons.UI.Shops;
using UnityEngine;
using Zenject;

namespace Raccoons.Products
{
    public class DemoShopInstaller : MonoInstaller
    {
        [Header("Shop")]
        [SerializeField] private ShopItemsRegistry shopItemsRegistry;
        [SerializeField] private ShopScreenStateController shopScreenStateController;

        [Header("Storage & Score")]
        [SerializeField] private string storageKey = "Game";
        [SerializeField] private string scoreKey = "Score";
        [Tooltip("Leave empty to skip encryption")]
        [SerializeField] private AesEncryptionAsset encryption;

        public override void InstallBindings()
        {
            InstallStorage();
            InstallShop();
            InstallEquippedItems();
        }

        private void InstallStorage()
        {
            var playerPrefsStorage = new PlayerPrefsStorage(storageKey);
            IStorageChannel storageChannel = encryption != null
                ? new EncryptedStorageChannel(playerPrefsStorage, encryption, encryption, storageKey)
                : playerPrefsStorage;

            Container.Bind<IStorageChannel>().FromInstance(storageChannel).AsSingle();

            IScoreStorage scoreStorage = new DefaultScoreStorage(scoreKey, storageChannel);
            var scoreBank = new MultipliedScoreBank(scoreStorage);
            Container.Bind<IScoreBank>().FromInstance(scoreBank).AsSingle();

            Container.Bind<ISerializer>().FromInstance(new NewtonsoftJsonSerializer()).AsSingle();
        }

        private void InstallShop()
        {
            Container.BindInterfacesAndSelfTo<ShopService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ShopItemsRegistry>().FromInstance(shopItemsRegistry).AsSingle();
            Container.Bind<ShopScreenStateController>().FromInstance(shopScreenStateController).AsSingle();
            Container.BindInterfacesAndSelfTo<SkinShopPurchaseHandler>().AsSingle();
        }

        private void InstallEquippedItems()
        {
            Container.BindInterfacesAndSelfTo<EquippedItemsService>().AsSingle().NonLazy();
        }
    }
}
