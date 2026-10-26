#if RACCOONS_FIREBASE_ANALYTICS
using Firebase;
using Firebase.Extensions;
#endif
using UnityEngine;

namespace Raccoons.Analytics.Firebase
{
    public class FirebaseAnalyticsInitializer : MonoBehaviour
    {
#pragma warning disable 0414 // unused while RACCOONS_FIREBASE_ANALYTICS is off
        // Opt-in: components already placed in live games' scenes must keep sending what they sent.
        [Tooltip("Wrap Firebase in FilteredAnalyticsService (drops *_load, *_load_fail, *_revenue_paid). " +
                 "Turn on for new games; for a live game only together with live-ops.")]
        [SerializeField] private bool _filterNoisyEvents = false;
#pragma warning restore 0414

        private void Awake()
        {
#if RACCOONS_FIREBASE_ANALYTICS
            RaccoonsAnalytics.HoldEvents();

            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                try
                {
                    if (!task.IsFaulted && !task.IsCanceled && task.Result == DependencyStatus.Available)
                    {
                        IAnalyticsService service = new FirebaseAnalyticsService();
                        if (_filterNoisyEvents) service = new FilteredAnalyticsService(service);
                        RaccoonsAnalytics.Initialize(service);
                    }
                    else
                    {
                        var reason = task.IsFaulted ? task.Exception?.GetBaseException().Message : task.IsCanceled ? "canceled" : task.Result.ToString();
                        Debug.LogError($"[FirebaseAnalyticsInitializer] Firebase unavailable: {reason}. Analytics disabled.");
                    }
                }
                finally
                {
                    // Held events must never pile up in memory, even when Firebase failed.
                    RaccoonsAnalytics.ReleaseEvents();
                }
            });
#else
            Debug.LogError("[FirebaseAnalyticsInitializer] RACCOONS_FIREBASE_ANALYTICS is not defined for this platform: " +
                           "NO events reach Firebase. Raccoons → Integrations → Enable with 'Apply to all platforms'.");
#endif
        }
    }
}
