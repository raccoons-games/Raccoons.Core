using UnityEngine;

namespace Raccoons.Storage.Files.Instances
{
    [CreateAssetMenu(fileName = "PersistentFolderStorageAsset", menuName = "Raccoons/Storage/PersistentFolderStorageAsset")]
    public class PersistentFolderStorageAsset : BaseFileStorageAsset
    {
        protected override IStorage CreateStorage(string key, IStorage parent, FileStorageSettings settings)
        {
            if (string.IsNullOrEmpty(key))
            {
                return new PersistentFolderStorage(settings);
            }
            else
            {
                return new FolderStorage(key, settings, new PersistentFolderStorage());
            }
        }
    }
}