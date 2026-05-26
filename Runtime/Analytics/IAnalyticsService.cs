using System.Collections.Generic;

namespace Raccoons.Analytics
{
    public interface IAnalyticsService
    {
        void LogEvent(string eventName, Dictionary<string, object> parameters);
    }
}
