using System.Collections.Generic;

namespace Raccoons.Analytics
{
    public static partial class RaccoonsAnalytics
    {
        public static void NewPlayer(Dictionary<string, object> additionalData = null)
        {
            Schedule("new_player", additionalData ?? new Dictionary<string, object>());
        }

        [System.Obsolete("GameStart is deprecated. game_started is sent automatically on Initialize.")]
        public static void GameStart() { }
    }
}
