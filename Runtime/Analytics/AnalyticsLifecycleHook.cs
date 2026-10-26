using UnityEngine;

namespace Raccoons.Analytics
{
    // Sends open banner batches before the app may be killed in the background.
    internal sealed class AnalyticsLifecycleHook : MonoBehaviour
    {
        private static AnalyticsLifecycleHook _instance;

        internal static void Ensure()
        {
            if (_instance != null || !Application.isPlaying) return;
            var go = new GameObject("[RaccoonsAnalytics]") { hideFlags = HideFlags.HideInHierarchy };
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<AnalyticsLifecycleHook>();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) RaccoonsAnalytics.FlushAdImpressionBatches();
        }

        private void OnApplicationQuit()
        {
            RaccoonsAnalytics.FlushAdImpressionBatches();
        }
    }
}
