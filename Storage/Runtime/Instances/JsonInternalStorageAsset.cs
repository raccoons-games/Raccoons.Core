using Raccoons.Storage.Instances;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raccoons.Storage
{
    [CreateAssetMenu(fileName = "JsonInternalStorageAsset", menuName = "Raccoons/Storage/JsonInternalStorageAsset")]
    public class JsonInternalStorageAsset : BaseStorageAsset
    {
        protected override IStorage CreateStorage(string key, IStorage parent)
        {
            return new JsonInternalStorage(key, parent);
        }
    }
}