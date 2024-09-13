using UnityEngine;

namespace Raccoons.Storage.Instances
{
    [CreateAssetMenu(fileName = "PlayerPrefsStorageAsset", menuName = "Raccoons/Storage/PlayerPrefsStorageAsset")]
    public class PlayerPrefsStorageAsset : BaseStorageAsset
    {
        protected override IStorage CreateStorage(string key, IStorage parent)
        {
            return new PlayerPrefsStorage(key, parent);
        }
    }
}