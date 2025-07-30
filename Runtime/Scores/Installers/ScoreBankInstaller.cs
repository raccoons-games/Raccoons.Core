using JetBrains.Annotations;
using Raccoons.Identifiers.Guids;
using Raccoons.Scores.Assets;
using Raccoons.Scores.Banks;
using Raccoons.Scores.Storages;
using Raccoons.Storage;
using Raccoons.Storage.Cryptography;
using Raccoons.Storage.Cryptography.Aes;
using UnityEngine;
using Zenject;

namespace Raccoons.Scores.Installers
{
    public class ScoreBankInstaller : MonoInstaller
    {
        [SerializeField] private GuidAsset key;
        
        [Header("Optional")]
        [SerializeField] private ScoreMetadataAsset metadata;
        
        [Tooltip("Leave empty if you don't want to encrypt it")]
        [SerializeField] private AesEncryptionAsset encryption;

        public override void InstallBindings()
        {
            var playerPrefsStorage = new PlayerPrefsStorage("Scores");

            IStorageChannel storageChannel = playerPrefsStorage;
            if (encryption != null)
            {
                storageChannel = new EncryptedStorageChannel(playerPrefsStorage, encryption, encryption, "S");
            }

            IScoreStorage scoreStorage = new DefaultScoreStorage(key.name, storageChannel);
            MultipliedScoreBank scoreBank = new MultipliedScoreBank(scoreStorage);
            if (metadata != null)
                scoreBank.SetMetadata(metadata.Metadata);

            Container.Bind<IScoreBank>().WithId(key).FromInstance(scoreBank).AsSingle();
        }
    }
}