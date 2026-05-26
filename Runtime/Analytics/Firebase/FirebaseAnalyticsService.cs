#if RACCOONS_FIREBASE_ANALYTICS
using Firebase.Analytics;
#endif
using System.Collections.Generic;

namespace Raccoons.Analytics.Firebase
{
    public class FirebaseAnalyticsService : IAnalyticsService
    {
        public void LogEvent(string eventName, Dictionary<string, object> parameters)
        {
#if RACCOONS_FIREBASE_ANALYTICS
            var firebaseParams = BuildFirebaseParams(parameters);
            FirebaseAnalytics.LogEvent(eventName, firebaseParams);
#endif
        }

#if RACCOONS_FIREBASE_ANALYTICS
        private static Parameter[] BuildFirebaseParams(Dictionary<string, object> parameters)
        {
            var result = new Parameter[parameters.Count];
            int i = 0;
            foreach (var kv in parameters)
                result[i++] = ToFirebaseParameter(kv.Key, kv.Value);
            return result;
        }

        private static Parameter ToFirebaseParameter(string key, object value)
        {
            return value switch
            {
                string s    => new Parameter(key, s),
                int n       => new Parameter(key, (long)n),
                long l      => new Parameter(key, l),
                float f     => new Parameter(key, (double)f),
                double d    => new Parameter(key, d),
                bool b      => new Parameter(key, b ? 1L : 0L),
                null        => new Parameter(key, string.Empty),
                _           => new Parameter(key, value.ToString())
            };
        }
#endif
    }
}
