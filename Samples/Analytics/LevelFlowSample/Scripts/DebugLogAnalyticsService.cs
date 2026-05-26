using System.Collections.Generic;
using System.Text;
using Raccoons.Analytics;
using UnityEngine;

namespace Raccoons.Core.Samples.Analytics
{
    public class DebugLogAnalyticsService : IAnalyticsService
    {
        public void LogEvent(string eventName, Dictionary<string, object> parameters)
        {
            Debug.Log($"[Analytics] {eventName}  {FormatParams(parameters)}");
        }

        private static string FormatParams(Dictionary<string, object> parameters)
        {
            var sb = new StringBuilder("{");
            foreach (var kv in parameters)
                sb.Append($" {kv.Key}: {kv.Value},");
            if (sb.Length > 1) sb.Length -= 1;
            sb.Append(" }");
            return sb.ToString();
        }
    }
}
