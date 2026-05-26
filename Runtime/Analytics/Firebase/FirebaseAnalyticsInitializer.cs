#if RACCOONS_FIREBASE_ANALYTICS
using Firebase;
using Firebase.Extensions;
#endif
using UnityEngine;

namespace Raccoons.Analytics.Firebase
{
    public class FirebaseAnalyticsInitializer : MonoBehaviour
    {
        private void Awake()
        {
            RaccoonsAnalytics.HoldEvents();

#if RACCOONS_FIREBASE_ANALYTICS
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                if (task.Result == DependencyStatus.Available)
                {
                    RaccoonsAnalytics.Initialize(new FirebaseAnalyticsService());
                    RaccoonsAnalytics.ReleaseEvents();
                }
                else
                {
                    Debug.LogError($"[FirebaseAnalyticsInitializer] Firebase unavailable: {task.Result}. Analytics disabled.");
                }
            });
#else
            Debug.LogWarning("[FirebaseAnalyticsInitializer] RACCOONS_FIREBASE_ANALYTICS is not defined. Enable it in Raccoons/Integrations.");
#endif
        }
    }
}
