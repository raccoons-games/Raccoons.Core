using System.Collections.Generic;

namespace Raccoons.Analytics
{
    public static partial class RaccoonsAnalytics
    {
        #region Interstitial

        public static void InterstitialLoad(string placement, string network, int? level = null, Dictionary<string, object> additionalData = null)
        {
            Schedule("interstitial_load", BuildAdParams(placement, network, level, additionalData));
        }

        public static void InterstitialLoad(AdEventArgs args, Dictionary<string, object> additionalData = null)
        {
            InterstitialLoad(args.Placement, args.Network, args.Level, additionalData);
        }

        public static void InterstitialLoadFail(string network, AdErrorType reason, int? level = null, Dictionary<string, object> additionalData = null)
        {
            var p = BuildAdParams(null, network, level, additionalData);
            p["error_reason"] = reason.ToString();
            Schedule("interstitial_load_fail", p);
        }

        public static void InterstitialLoadFail(AdFailEventArgs args, Dictionary<string, object> additionalData = null)
        {
            InterstitialLoadFail(args.Network, args.Reason, args.Level, additionalData);
        }

        public static void InterstitialTryShow(string placement, Dictionary<string, object> additionalData = null)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            p["placement"] = placement;
            Schedule("interstitial_try_show", p);
        }

        public static void InterstitialShowRequested(string placement, string network, int? level = null, Dictionary<string, object> additionalData = null)
        {
            Schedule("interstitial_show_requested", BuildAdParams(placement, network, level, additionalData));
        }

        public static void InterstitialShow(string placement, string network, int? level = null, Dictionary<string, object> additionalData = null)
        {
            Schedule("interstitial_show", BuildAdParams(placement, network, level, additionalData));
        }

        public static void InterstitialShowFail(string placement, string network, int? level = null, AdErrorType reason = AdErrorType.Undefined, Dictionary<string, object> additionalData = null)
        {
            var p = BuildAdParams(placement, network, level, additionalData);
            p["error_reason"] = reason.ToString();
            Schedule("interstitial_show_fail", p);
        }

        public static void InterstitialEnd(string placement, string network, int? level = null, Dictionary<string, object> additionalData = null)
        {
            Schedule("interstitial_end", BuildAdParams(placement, network, level, additionalData));
        }

        public static void InterstitialClick(string placement, string network, int? level = null, Dictionary<string, object> additionalData = null)
        {
            Schedule("interstitial_click", BuildAdParams(placement, network, level, additionalData));
        }

        public static void InterstitialRevenuePaid(double revenue, string placement, string network, int? level = null, Dictionary<string, object> additionalData = null)
        {
            var p = BuildAdParams(placement, network, level, additionalData);
            p["revenue"] = revenue;
            Schedule("interstitial_revenue_paid", p);
        }

        public static void InterstitialRevenuePaid(AdRevenueEventArgs args, Dictionary<string, object> additionalData = null)
        {
            InterstitialRevenuePaid(args.Revenue, args.Placement, args.Network, args.Level, additionalData);
        }

        #endregion

        #region Rewarded Video

        public static void RewardVideoLoad(string placement, string network, int? level = null, Dictionary<string, object> additionalData = null)
        {
            Schedule("reward_video_load", BuildAdParams(placement, network, level, additionalData));
        }

        public static void RewardVideoLoad(AdEventArgs args, Dictionary<string, object> additionalData = null)
        {
            RewardVideoLoad(args.Placement, args.Network, args.Level, additionalData);
        }

        public static void RewardVideoLoadFail(string network, int? level = null, AdErrorType reason = AdErrorType.Undefined, string placement = null, Dictionary<string, object> additionalData = null)
        {
            var p = BuildAdParams(placement, network, level, additionalData);
            p["error_reason"] = reason.ToString();
            Schedule("reward_video_load_fail", p);
        }

        public static void RewardVideoTryShow(string placement, Dictionary<string, object> additionalData = null)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            p["placement"] = placement;
            Schedule("reward_video_try_show", p);
        }

        public static void RewardVideoShowRequested(string placement, string network, int? level = null, Dictionary<string, object> additionalData = null)
        {
            Schedule("reward_video_show_requested", BuildAdParams(placement, network, level, additionalData));
        }

        public static void RewardVideoShow(string placement, string network, int? level = null, Dictionary<string, object> additionalData = null)
        {
            Schedule("reward_video_show", BuildAdParams(placement, network, level, additionalData));
        }

        public static void RewardVideoShowFail(string placement, string network, AdErrorType reason, int? level = null, Dictionary<string, object> additionalData = null)
        {
            var p = BuildAdParams(placement, network, level, additionalData);
            p["error_reason"] = reason.ToString();
            Schedule("reward_video_show_fail", p);
        }

        public static void RewardVideoEnd(string placement, string network, int? level = null, Dictionary<string, object> additionalData = null)
        {
            Schedule("reward_video_end", BuildAdParams(placement, network, level, additionalData));
        }

        public static void RewardVideoClick(string placement, string network, int? level = null, Dictionary<string, object> additionalData = null)
        {
            Schedule("reward_video_click", BuildAdParams(placement, network, level, additionalData));
        }

        public static void RewardVideoCollect(string placement, Reward reward = null, int? level = null, Dictionary<string, object> additionalData = null)
        {
            var p = BuildAdParams(placement, null, level, additionalData);
            if (reward != null) MergeParams(p, reward.ToParameters());
            Schedule("reward_video_collect", p);
        }

        public static void RewardVideoCollect(AdRewardArgs args, Dictionary<string, object> additionalData = null)
        {
            RewardVideoCollect(args.Placement, args.Reward, args.Level, additionalData);
        }

        public static void RewardVideoOpportunity(string placement, Dictionary<string, object> additionalData = null)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            p["placement"] = placement;
            Schedule("reward_video_opportunity", p);
        }

        public static void RewardVideoRevenuePaid(double revenue, string placement, string network, int? level = null, Dictionary<string, object> additionalData = null)
        {
            var p = BuildAdParams(placement, network, level, additionalData);
            p["revenue"] = revenue;
            Schedule("reward_video_revenue_paid", p);
        }

        public static void RewardVideoRevenuePaid(AdRevenueEventArgs args, Dictionary<string, object> additionalData = null)
        {
            RewardVideoRevenuePaid(args.Revenue, args.Placement, args.Network, args.Level, additionalData);
        }

        #endregion

        #region Banner

        public static void BannerLoad(string placement, string network, Dictionary<string, object> additionalData = null)
        {
            Schedule("banner_load", BuildAdParams(placement, network, null, additionalData));
        }

        public static void BannerLoad(BannerEventArgs args, Dictionary<string, object> additionalData = null)
        {
            BannerLoad(args.Placement, args.Network, additionalData);
        }

        public static void BannerLoadFail(string network, AdErrorType reason, Dictionary<string, object> additionalData = null)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            p["ad_provider"] = network;
            p["error_reason"] = reason.ToString();
            Schedule("banner_load_fail", p);
        }

        public static void BannerLoadFail(BannerFailEventArgs args, Dictionary<string, object> additionalData = null)
        {
            BannerLoadFail(args.Network, args.Reason, additionalData);
        }

        public static void BannerShowRequested(string placement, string network, Dictionary<string, object> additionalData = null)
        {
            Schedule("banner_show_requested", BuildAdParams(placement, network, null, additionalData));
        }

        public static void BannerShow(string placement, string network, Dictionary<string, object> additionalData = null)
        {
            Schedule("banner_show", BuildAdParams(placement, network, null, additionalData));
        }

        public static void BannerShowFail(string placement, string network, AdErrorType reason, Dictionary<string, object> additionalData = null)
        {
            var p = BuildAdParams(placement, network, null, additionalData);
            p["error_reason"] = reason.ToString();
            Schedule("banner_show_fail", p);
        }

        public static void BannerHide(string placement, string network, Dictionary<string, object> additionalData = null)
        {
            Schedule("banner_hide", BuildAdParams(placement, network, null, additionalData));
        }

        public static void BannerRevenuePaid(double revenue, string placement, string network, Dictionary<string, object> additionalData = null)
        {
            var p = BuildAdParams(placement, network, null, additionalData);
            p["revenue"] = revenue;
            Schedule("banner_revenue_paid", p);
        }

        #endregion

        private static Dictionary<string, object> BuildAdParams(string placement, string network, int? level, Dictionary<string, object> additionalData)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            if (placement != null) p["placement"] = placement;
            if (network != null) p["ad_provider"] = network;
            if (level.HasValue) p["level_num"] = level.Value;
            return p;
        }
    }
}
