using System;
using System.Collections.Generic;
using System.Threading;

namespace Raccoons.Analytics
{
    // Ad revenue for the live-ops dashboard. Mediation-agnostic: AppLovin MAX is wired by
    // Raccoons.Analytics.AppLovinMax.MaxAnalyticsBridge, any other mediation calls these directly.
    // Safe to call from any thread.
    public static partial class RaccoonsAnalytics
    {
        /// <summary>
        /// 0 = one ad_impression per banner/leader impression (what the dashboard reads today).
        /// &gt;0 = banner/leader impressions are summed over this many seconds into one ad_impression with
        /// `impressions` / `impressions_with_value`. Revenue stays exact, but impression counts, banner eCPM
        /// and fill in the dashboard are wrong until rg-liveops reads `impressions`. Target: 300.
        /// </summary>
        public static int BannerBatchSeconds { get; set; }

        /// <summary>eCPM of the first interstitial of this app launch, once captured.</summary>
        public static double? SessionInitialEcpm
        {
            get { lock (_adLock) return _initialEcpm; }
        }

        private static readonly object _adLock = new();
        private static readonly Dictionary<string, BannerBatch> _bannerBatches = new();
        private static int _initialEcpmSent;
        private static double? _initialEcpm;

        private sealed class BannerBatch
        {
            public string Format;
            public string AdUnit;
            public string Platform;
            public double Revenue;
            public long Impressions;
            public long ImpressionsWithValue;
            public DateTime StartedUtc;
        }

        /// <summary>
        /// One paid ad impression → ad_impression, the only source of ad revenue for the dashboard.
        /// Do not also send *RevenuePaid events: the dashboard would count the money twice.
        /// </summary>
        /// <param name="adFormat">Mediation format code, uppercase: BANNER, LEADER, MREC, INTER, REWARDED.</param>
        /// <param name="revenue">USD. Negative (MAX sends -1 when the network did not disclose it) is reported as 0.</param>
        /// <param name="placement">placement_* passed to the show call; null for banners.</param>
        public static void AdImpression(string adFormat, double revenue, string placement = null,
            string adSource = null, string adUnitName = null, string adPlatform = "AppLovin")
        {
            var format = (adFormat ?? string.Empty).ToUpperInvariant();
            var value = revenue < 0 || double.IsNaN(revenue) ? 0d : revenue;

            if (BannerBatchSeconds > 0 && (format == "BANNER" || format == "LEADER"))
            {
                AddToBannerBatch(format, adUnitName, adPlatform, value);
                return;
            }

            ScheduleLight("ad_impression", new Dictionary<string, object>
            {
                ["ad_platform"] = adPlatform ?? string.Empty,
                ["ad_source"] = adSource ?? string.Empty,
                ["ad_unit_name"] = adUnitName ?? string.Empty,
                ["ad_format"] = format,
                ["value"] = value,
                ["currency"] = "USD",
                ["placement"] = placement ?? string.Empty,
            });

            // Fallback for networks that disclose revenue only on the impression, not on load.
            if (format == "INTER" && revenue >= 0) TrySendInitialEcpm(revenue, adSource, adUnitName, adPlatform);
        }

        /// <summary>
        /// An ad finished loading. Only interstitials matter: the first one per launch becomes
        /// initial_ecpm (revenue × 1000), which the dashboard uses for eCPM tiers.
        /// </summary>
        public static void AdLoaded(string adFormat, double revenue, string adSource = null,
            string adUnitName = null, string adPlatform = "AppLovin")
        {
            if (revenue < 0 || !string.Equals(adFormat, "INTER", StringComparison.OrdinalIgnoreCase)) return;
            TrySendInitialEcpm(revenue, adSource, adUnitName, adPlatform);
        }

        /// <summary>
        /// Sends banner batches that are still open. Called automatically on app pause and quit;
        /// no-op while BannerBatchSeconds is 0.
        /// </summary>
        public static void FlushAdImpressionBatches()
        {
            List<BannerBatch> batches;
            lock (_adLock)
            {
                if (_bannerBatches.Count == 0) return;
                batches = new List<BannerBatch>(_bannerBatches.Values);
                _bannerBatches.Clear();
            }
            foreach (var b in batches) SendBannerBatch(b);
        }

        private static void ResetAdRevenue()
        {
            lock (_adLock)
            {
                _bannerBatches.Clear();
                _initialEcpm = null;
            }
            _initialEcpmSent = 0;
            BannerBatchSeconds = 0;
        }

        private static void TrySendInitialEcpm(double revenue, string adSource, string adUnitName, string adPlatform)
        {
            if (Interlocked.CompareExchange(ref _initialEcpmSent, 1, 0) != 0) return;
            var ecpm = revenue * 1000.0;
            lock (_adLock) _initialEcpm = ecpm;

            ScheduleLight("initial_ecpm", new Dictionary<string, object>
            {
                ["value"] = ecpm,
                ["currency"] = "USD",
                ["ad_platform"] = adPlatform ?? string.Empty,
                ["ad_source"] = adSource ?? string.Empty,
                ["ad_unit_name"] = adUnitName ?? string.Empty,
            });
        }

        private static void AddToBannerBatch(string format, string adUnit, string platform, double value)
        {
            BannerBatch ready = null;
            var key = format + "|" + adUnit;
            var now = DateTime.UtcNow; // not Time.*: may run off the main thread
            lock (_adLock)
            {
                if (!_bannerBatches.TryGetValue(key, out var b))
                {
                    b = new BannerBatch { Format = format, AdUnit = adUnit, Platform = platform, StartedUtc = now };
                    _bannerBatches[key] = b;
                }
                b.Revenue += value;
                b.Impressions++;
                if (value > 0) b.ImpressionsWithValue++;
                if ((now - b.StartedUtc).TotalSeconds >= BannerBatchSeconds)
                {
                    ready = b;
                    _bannerBatches.Remove(key);
                }
            }
            if (ready != null) SendBannerBatch(ready);
        }

        private static void SendBannerBatch(BannerBatch b)
        {
            ScheduleLight("ad_impression", new Dictionary<string, object>
            {
                ["ad_platform"] = b.Platform ?? string.Empty,
                ["ad_unit_name"] = b.AdUnit ?? string.Empty,
                ["ad_format"] = b.Format,
                ["value"] = b.Revenue,
                ["currency"] = "USD",
                ["placement"] = string.Empty,
                ["impressions"] = b.Impressions,
                ["impressions_with_value"] = b.ImpressionsWithValue,
            });
        }
    }
}
