using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raccoons.Identifiers.Guids
{
    [Serializable]
    public abstract class BaseGuidAssetList<TGuid> : IGuidAssetList where TGuid: IGuid
    {
        [SerializeField]
        private List<TGuid> guids;

        public List<TGuid> Guids { get => guids; }

        public BaseGuidAssetList()
        {
            guids = new List<TGuid>();
        }

        public HashSet<IGuid> GetGuidsSet()
        {
            HashSet<IGuid> result = new HashSet<IGuid>((IEnumerable<IGuid>)guids);
            return result;
        }
        public bool IsSubsetOf(IGuidAssetList compareTo)
        {
            return GetGuidsSet().IsSubsetOf(compareTo.GetGuidsSet());
        }
        public bool Contains(IGuidAssetList compareTo)
        {
            bool result = false;
            var tags = GetGuidsSet();
            foreach (var tag in compareTo.GetGuidsSet())
            {
                if (tags.Contains(tag) == true)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }
        public bool ContainsGuid(IGuid guid)
        {
            return GetGuidsSet().Contains(guid);
        }
    }
}
