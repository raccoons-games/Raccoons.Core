using UnityEngine;

namespace Raccoons.UI.Shops
{
    public class ShopScreenStateController : MonoBehaviour
    {
        [SerializeField]
        private ShopScreen shopScreen;

        public void Show() => shopScreen.Show();

        public void Hide() => shopScreen.Hide();
    }
}
