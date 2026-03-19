using UnityEngine;

namespace Raccoons.UI.Shops
{
    public abstract class BaseRedDotComponent : MonoBehaviour
    {
        [SerializeField] private GameObject dot;

        protected virtual void OnEnable() => UpdateState();

        public abstract bool IsDotActive();

        protected void UpdateState()
        {
            if (dot != null)
                dot.SetActive(IsDotActive());
        }
    }
}
