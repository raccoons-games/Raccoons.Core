using System.Collections.Generic;

namespace Raccoons.Core.Runtime.Shop
{
    [System.Serializable]
    public class EquippedItemsSaveData
    {
        public List<string> PurchasedItems = new();
        public Dictionary<string, string> CurrentItems = new();

        public void AddPurchasedItem(string itemId)
            => PurchasedItems.Add(itemId);
    }
}
