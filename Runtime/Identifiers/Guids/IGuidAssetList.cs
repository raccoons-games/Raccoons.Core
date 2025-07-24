using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raccoons.Identifiers.Guids
{
    public interface IGuidAssetList
    {
         HashSet<IGuid> GetGuidsSet();
    }
}
