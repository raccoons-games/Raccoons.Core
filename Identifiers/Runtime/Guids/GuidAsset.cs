using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raccoons.Identifiers.Guids
{
    [CreateAssetMenu(fileName = "GuidAsset", menuName = "Raccoons/Identifiers/Guids/GuidAsset")]
    public class GuidAsset : ScriptableObject, IGuid
    {
        public string Key { get => name; }
    }
}
