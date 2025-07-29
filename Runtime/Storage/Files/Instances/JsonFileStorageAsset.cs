using Raccoons.Files.Instances;
using Raccoons.Storage.Instances;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raccoons.Storage.Files.Instances
{
    [CreateAssetMenu(fileName = "JsonFileStorageAsset", menuName = "Raccoons/Storage/JsonFileStorageAsset")]
    public class JsonFileStorageAsset : BaseFileStorageAsset
    {
        [Tooltip("Optinal file asset instance that overrides default path from parent")]
        [SerializeField]
        private BaseFileAsset fileAsset;

        protected override IStorage CreateStorage(string key, IStorage parent, FileStorageSettings settings)
        {
            if (fileAsset == null)
            {
                return new JsonFileStorage(key, settings, parent);
            }
            else
            {
                return new JsonFileStorage(Key, fileAsset.Reader, fileAsset.Writer, settings);
            }
        }
    }
}