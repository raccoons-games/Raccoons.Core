using UnityEngine;

namespace Raccoons.Storage.Files.Instances
{
    [CreateAssetMenu(fileName = "FolderStorageAsset", menuName = "Raccoons/Storage/FolderStorageAsset")]
    public class FolderStorageAsset : BaseFileStorageAsset
    {
        protected override IStorage CreateStorage(string key, IStorage parent, FileStorageSettings settings)
        {
            return new FolderStorage(key, settings, parent);
        }
    }
}