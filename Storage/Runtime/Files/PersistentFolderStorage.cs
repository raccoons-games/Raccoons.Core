using UnityEngine;

namespace Raccoons.Storage.Files
{
    /// <summary>
    /// Folder storage that represents persistent folder
    /// </summary>
    public class PersistentFolderStorage : FolderStorage
    {
        public PersistentFolderStorage(FileStorageSettings settings) 
            : base(Application.persistentDataPath, settings, null)
        {
        }

        public PersistentFolderStorage()
            : base(Application.persistentDataPath, new FileStorageSettings(), null)
        {
        }
    }
}