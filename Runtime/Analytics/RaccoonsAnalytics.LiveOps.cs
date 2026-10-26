using System.Collections.Generic;

namespace Raccoons.Analytics
{
    // Events the live-ops dashboard reads that the Lion spec does not cover.
    // Names and parameter types are a contract with rg-liveops (docs/analytics-event-contract.md).
    public static partial class RaccoonsAnalytics
    {
        /// <summary>
        /// One event per coin transaction, never per coin or animation tick.
        /// Earned = positive delta, spent = negative delta.
        /// </summary>
        public static void CoinsDelta(int delta, string source, Dictionary<string, object> additionalData = null)
        {
            if (delta == 0) return;
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            p["direction"] = delta > 0 ? "earned" : "spent";
            p["delta"] = delta;
            p["source"] = string.IsNullOrEmpty(source) ? "unknown" : source;
            Schedule("coins_delta", p);
        }

        /// <summary>
        /// A revive on a failed level, whatever paid for it. Once a game sends revive_used, the dashboard
        /// takes it as THE revive signal, so send it on every revive and never also as PowerUpUsed.
        /// </summary>
        public static void ReviveUsed(int level, string source, Dictionary<string, object> additionalData = null)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            p["level"] = level;
            p["source"] = string.IsNullOrEmpty(source) ? "unknown" : source;
            Schedule("revive_used", p);
        }

        /// <summary>
        /// open_tutorial_step_&lt;step&gt;_start|finish. The step number is the one dynamic fragment the dashboard
        /// parses from an event name. Steps start at 1; stepCount is the number of the last step.
        /// </summary>
        public static void TutorialStep(string track, int step, int stepCount, bool finished, string variant = "none")
        {
            Schedule($"open_tutorial_step_{step}_{(finished ? "finish" : "start")}", new Dictionary<string, object>
            {
                ["tutorial"] = string.IsNullOrEmpty(track) ? "main" : track,
                ["step_count"] = stepCount,
                ["variant"] = string.IsNullOrEmpty(variant) ? "none" : variant,
            });
        }
    }
}
