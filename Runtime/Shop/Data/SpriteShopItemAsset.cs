using UnityEngine;

namespace Raccoons.Core.Runtime.Shop
{
    [CreateAssetMenu(fileName = "SpriteShopItem", menuName = "Raccoons/Shop/SpriteShopItem")]
    public class SpriteShopItemAsset : BaseShopItemAsset
    {
        [SerializeField] private Sprite background;
        public Sprite Background => background;
    }
}
