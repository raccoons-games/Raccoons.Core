using UnityEngine;

namespace Raccoons.Core.Runtime.Shop
{
    public class ShopScreenStateController : MonoBehaviour
    {
        [SerializeField]
        private ShopScreen shopScreen;

        public void Show() => shopScreen.Show();

        public void Hide() => shopScreen.Hide();
    }
}
