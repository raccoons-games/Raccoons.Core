namespace Raccoons.Analytics.AppLovinMax
{
    /// <summary>
    /// Feeds AppLovin MAX ad revenue into RaccoonsAnalytics: ad_impression for every format and
    /// initial_ecpm from the first loaded interstitial. Compiled only when the MAX UPM package
    /// (com.applovin.mediation.ads) is installed; with MAX as a .unitypackage add the scripting
    /// define RACCOONS_MAX_AVAILABLE. Not attached automatically, so a game that already logs
    /// ad_impression itself does not count revenue twice.
    /// </summary>
    public static class MaxAnalyticsBridge
    {
        private static bool _attached;

        /// <summary>Call once, e.g. right after MaxSdk.InitializeSdk().</summary>
        public static void Attach()
        {
            if (_attached) return;
            _attached = true;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaid;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaid;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaid;
            MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnAdRevenuePaid;
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoaded;
        }

        public static void Detach()
        {
            if (!_attached) return;
            _attached = false;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent -= OnAdRevenuePaid;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent -= OnAdRevenuePaid;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent -= OnAdRevenuePaid;
            MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent -= OnAdRevenuePaid;
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent -= OnInterstitialLoaded;
        }

        // MAX may raise these off the main thread; RaccoonsAnalytics.AdImpression is thread-safe.
        private static void OnAdRevenuePaid(string adUnitId, MaxSdkBase.AdInfo info)
        {
            if (info == null) return;
            RaccoonsAnalytics.AdImpression(info.AdFormat, info.Revenue, info.Placement,
                info.NetworkName, info.AdUnitIdentifier ?? adUnitId);
        }

        private static void OnInterstitialLoaded(string adUnitId, MaxSdkBase.AdInfo info)
        {
            if (info == null) return;
            RaccoonsAnalytics.AdLoaded(info.AdFormat, info.Revenue, info.NetworkName, info.AdUnitIdentifier ?? adUnitId);
        }
    }
}
