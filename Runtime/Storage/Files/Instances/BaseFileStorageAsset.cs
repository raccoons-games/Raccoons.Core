using Raccoons.Storage.Instances;
using UnityEngine;

namespace Raccoons.Storage.Files.Instances
{
    public abstract class BaseFileStorageAsset : BaseStorageAsset
    {
        [SerializeField]
        private FileStorageSettings settings;

        protected override IStorage CreateStorage(string key, IStorage parent)
        {
            return CreateStorage(key, parent, settings);
        }

        protected abstract IStorage CreateStorage(string key, IStorage parent, FileStorageSettings settings);
    }
}