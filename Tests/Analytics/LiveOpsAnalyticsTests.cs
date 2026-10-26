using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Raccoons.Analytics.Tests
{
    public class LiveOpsAnalyticsTests
    {
        private sealed class RecordingService : IAnalyticsService
        {
            public readonly List<(string Name, Dictionary<string, object> Params)> Events = new();

            public void LogEvent(string eventName, Dictionary<string, object> parameters)
            {
                lock (Events) Events.Add((eventName, new Dictionary<string, object>(parameters)));
            }

            public List<Dictionary<string, object>> Named(string name)
            {
                lock (Events) return Events.Where(e => e.Name == name).Select(e => e.Params).ToList();
            }
        }

        private RecordingService _sink;

        [SetUp]
        public void SetUp()
        {
            RaccoonsAnalytics.ResetForTests();
            _sink = new RecordingService();
        }

        [TearDown]
        public void TearDown() => RaccoonsAnalytics.ResetForTests();

        [Test]
        public void Custom_GoesThroughPipeline_WithGlobalParams()
        {
            RaccoonsAnalytics.Initialize(_sink);
            RaccoonsAnalytics.SetPlayerLevel(7);
            RaccoonsAnalytics.Custom("level_quit", new Dictionary<string, object> { ["reason"] = "home" });

            var e = _sink.Named("level_quit").Single();
            Assert.AreEqual("home", e["reason"]);
            Assert.AreEqual(7, e["user_level"]);
            Assert.AreEqual("true", e["debug"]);
            Assert.IsTrue(e.ContainsKey("store"));
            Assert.IsTrue(e.ContainsKey("build_version"), "Initialize defaults build_version to Application.version");
        }

        [Test]
        public void HeldEvents_AreSentOnRelease()
        {
            RaccoonsAnalytics.HoldEvents();
            RaccoonsAnalytics.CoinsDelta(-5, "shop");
            RaccoonsAnalytics.AdImpression("INTER", 0.01, "placement_test", "Net", "unit");
            RaccoonsAnalytics.Initialize(_sink);
            Assert.IsEmpty(_sink.Named("coins_delta"));

            RaccoonsAnalytics.ReleaseEvents();
            Assert.AreEqual(1, _sink.Named("coins_delta").Count);
            var ad = _sink.Named("ad_impression").Single();
            Assert.IsTrue(ad["store"] is bool, "store is stamped at dispatch, after Initialize");
        }

        [Test]
        public void AdImpression_NegativeRevenue_IsReportedAsZero_AndKeepsLightParams()
        {
            RaccoonsAnalytics.Initialize(_sink);
            RaccoonsAnalytics.SetCurrencyBalance("soft", "coins", 100);
            RaccoonsAnalytics.AdImpression("inter", -1, "placement_level_complete_continue", "Net", "unit");

            var ad = _sink.Named("ad_impression").Single();
            Assert.AreEqual(0d, ad["value"]);
            Assert.AreEqual("INTER", ad["ad_format"]);
            Assert.AreEqual("placement_level_complete_continue", ad["placement"]);
            Assert.AreEqual("USD", ad["currency"]);
            Assert.IsTrue(ad.ContainsKey("debug") && ad.ContainsKey("store") && ad.ContainsKey("build_version"));
            Assert.IsFalse(ad.ContainsKey("currency_soft_coins"), "ad_impression carries no balances");
            Assert.IsEmpty(_sink.Named("initial_ecpm"), "undisclosed revenue gives no eCPM");
        }

        [Test]
        public void InitialEcpm_IsSentOncePerLaunch_FromLoadThenNotFromImpression()
        {
            RaccoonsAnalytics.Initialize(_sink);
            RaccoonsAnalytics.AdLoaded("INTER", 0.012, "NetA", "unit");
            RaccoonsAnalytics.AdLoaded("INTER", 0.5, "NetB", "unit");
            RaccoonsAnalytics.AdImpression("INTER", 0.02, "placement_x", "NetA", "unit");
            RaccoonsAnalytics.AdLoaded("REWARDED", 0.3, "NetA", "unit");

            var ecpm = _sink.Named("initial_ecpm").Single();
            Assert.AreEqual(12.0, (double)ecpm["value"], 1e-9);
            Assert.AreEqual(12.0, RaccoonsAnalytics.SessionInitialEcpm.Value, 1e-9);
        }

        [Test]
        public void Banners_PerImpression_ByDefault()
        {
            RaccoonsAnalytics.Initialize(_sink);
            for (var i = 0; i < 3; i++) RaccoonsAnalytics.AdImpression("BANNER", 0.0002, null, "Net", "b1");
            Assert.AreEqual(3, _sink.Named("ad_impression").Count);
        }

        [Test]
        public void Banners_Batched_SumRevenue_AndFlush()
        {
            RaccoonsAnalytics.Initialize(_sink);
            RaccoonsAnalytics.BannerBatchSeconds = 300;
            RaccoonsAnalytics.AdImpression("BANNER", -1, null, "Net", "b1");
            for (var i = 0; i < 3; i++) RaccoonsAnalytics.AdImpression("BANNER", 0.0002, null, "Net", "b1");
            RaccoonsAnalytics.AdImpression("LEADER", 0.0001, null, "Net", "l1");
            RaccoonsAnalytics.AdImpression("REWARDED", 0.05, "placement_rv", "Net", "r1");
            Assert.AreEqual(1, _sink.Named("ad_impression").Count, "only the rewarded one is sent immediately");

            RaccoonsAnalytics.FlushAdImpressionBatches();
            var banner = _sink.Named("ad_impression").Single(e => (string)e["ad_format"] == "BANNER");
            Assert.AreEqual(4L, banner["impressions"]);
            Assert.AreEqual(3L, banner["impressions_with_value"]);
            Assert.AreEqual(0.0006, (double)banner["value"], 1e-12);
            Assert.AreEqual(1, _sink.Named("ad_impression").Count(e => (string)e["ad_format"] == "LEADER"));
        }

        [Test]
        public void Banners_Batched_AreThreadSafe()
        {
            RaccoonsAnalytics.Initialize(_sink);
            RaccoonsAnalytics.BannerBatchSeconds = 300;
            Parallel.For(0, 1000, _ => RaccoonsAnalytics.AdImpression("BANNER", 0.001, null, "Net", "b1"));
            RaccoonsAnalytics.FlushAdImpressionBatches();

            var banner = _sink.Named("ad_impression").Single();
            Assert.AreEqual(1000L, banner["impressions"]);
            Assert.AreEqual(1.0, (double)banner["value"], 1e-9);
        }

        [Test]
        public void TypedEvents_MatchDashboardContract()
        {
            RaccoonsAnalytics.Initialize(_sink);
            RaccoonsAnalytics.CoinsDelta(50, "level_complete");
            RaccoonsAnalytics.CoinsDelta(-200, "booster_hint");
            RaccoonsAnalytics.CoinsDelta(0, "noop");
            RaccoonsAnalytics.ReviveUsed(12, "rewarded");
            RaccoonsAnalytics.TutorialStep("main", 2, 5, true, null);

            var coins = _sink.Named("coins_delta");
            Assert.AreEqual(2, coins.Count);
            Assert.AreEqual("earned", coins[0]["direction"]);
            Assert.AreEqual(50, coins[0]["delta"]);
            Assert.AreEqual("spent", coins[1]["direction"]);
            Assert.AreEqual(-200, coins[1]["delta"]);
            Assert.AreEqual(12, _sink.Named("revive_used").Single()["level"]);

            var step = _sink.Named("open_tutorial_step_2_finish").Single();
            Assert.AreEqual("main", step["tutorial"]);
            Assert.AreEqual(5, step["step_count"]);
            Assert.AreEqual("none", step["variant"]);
        }

        [Test]
        public void Filter_DropsDefaultAndExtraEvents()
        {
            var filter = new FilteredAnalyticsService(_sink, new[] { "input_mistap" });
            RaccoonsAnalytics.Initialize(filter);
            RaccoonsAnalytics.BannerLoad("placement_banner", "Net");
            RaccoonsAnalytics.InterstitialRevenuePaid(0.01, "placement_x", "Net");
            RaccoonsAnalytics.Custom("input_mistap");
            filter.Block("hint_side_shown");
            RaccoonsAnalytics.Custom("hint_side_shown");
            RaccoonsAnalytics.MissionStarted("level", "level_1", 1, 1);

            Assert.IsEmpty(_sink.Named("banner_load"));
            Assert.IsEmpty(_sink.Named("interstitial_revenue_paid"));
            Assert.IsEmpty(_sink.Named("input_mistap"));
            Assert.IsEmpty(_sink.Named("hint_side_shown"));
            Assert.AreEqual(1, _sink.Named("mission_started").Count);
        }
    }
}
