using System.Collections.Generic;

namespace Raccoons.Analytics
{
    public class AnalyticsEvent
    {
        public string EventName { get; }
        public IReadOnlyDictionary<string, object> Parameters { get; }

        public AnalyticsEvent(string eventName, IReadOnlyDictionary<string, object> parameters)
        {
            EventName = eventName;
            Parameters = parameters;
        }
    }
}
