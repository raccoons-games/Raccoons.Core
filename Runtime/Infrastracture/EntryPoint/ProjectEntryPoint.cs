using UnityEngine;

namespace Raccoons.Infrastructure
{
    public class ProjectEntryPoint : MonoBehaviour
    {
        private static bool _globalServicesInitialized;

        public static bool GlobalServicesInitialized => _globalServicesInitialized;

        internal static void MarkGlobalServicesInitialized()
        {
            _globalServicesInitialized = true;
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
