using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Raccoons.Products
{
    public class ShopItemsRegistry : MonoBehaviour
    {
        [SerializeField]
        private List<BaseShopItemAsset> items;

        public IEnumerable<BaseShopItemAsset> Items => items;

        public TItem GetShopItem<TItem>(string itemId) where TItem : BaseShopItemAsset
            => items.FirstOrDefault(item => item.Key == itemId) as TItem;

        public IEnumerable<BaseShopItemAsset> GetAllItemsOfCategory(ShopItemCategoryAsset category) =>
            items.Where(item => item.Category == category);

#if UNITY_EDITOR
        [ContextMenu("Get All Items From Project")]
        private void GetAllItemsFromProject()
        {
            string[] guids = AssetDatabase.FindAssets("t:BaseShopItemAsset");
            items = new List<BaseShopItemAsset>();

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                BaseShopItemAsset asset = AssetDatabase.LoadAssetAtPath<BaseShopItemAsset>(assetPath);
                if (asset != null)
                    items.Add(asset);
            }
        }
#endif
    }
}
