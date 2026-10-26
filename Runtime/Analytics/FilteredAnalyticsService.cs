using System.Collections.Generic;

namespace Raccoons.Analytics
{
    /// <summary>
    /// Wraps a service and drops events nobody reads, to keep the game under the 1M events/day
    /// GA4 → BigQuery export limit. Note: RaccoonsAnalytics.OnLogEvent fires before services, so a
    /// debug log still shows an event this filter drops.
    /// </summary>
    public sealed class FilteredAnalyticsService : IAnalyticsService
    {
        public static readonly IReadOnlyCollection<string> DefaultBlocked = new[]
        {
            // Ad loading diagnostics: ~30% of Arrow Cube's daily volume, read by no dashboard page.
            "interstitial_load", "interstitial_load_fail",
            "reward_video_load", "reward_video_load_fail",
            "banner_load", "banner_load_fail",
            // Revenue lives on ad_impression; these would double-count it.
            "interstitial_revenue_paid", "reward_video_revenue_paid", "banner_revenue_paid",
        };

        private readonly IAnalyticsService _inner;
        private readonly HashSet<string> _blocked;

        public FilteredAnalyticsService(IAnalyticsService inner, IEnumerable<string> extraBlocked = null)
        {
            _inner = inner;
            _blocked = new HashSet<string>(DefaultBlocked);
            if (extraBlocked != null) _blocked.UnionWith(extraBlocked);
        }

        /// <summary>Block one more event at runtime, e.g. from a Remote Config list.</summary>
        public void Block(string eventName)
        {
            lock (_blocked) _blocked.Add(eventName);
        }

        public void Unblock(string eventName)
        {
            lock (_blocked) _blocked.Remove(eventName);
        }

        public void LogEvent(string eventName, Dictionary<string, object> parameters)
        {
            lock (_blocked)
            {
                if (_blocked.Contains(eventName)) return;
            }
            _inner.LogEvent(eventName, parameters);
        }
    }
}
