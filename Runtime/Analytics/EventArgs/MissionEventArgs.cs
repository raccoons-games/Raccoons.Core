namespace Raccoons.Analytics
{
    public class MissionEventArgs
    {
        public string MissionType { get; set; }
        public string MissionName { get; set; }
        public int MissionID { get; set; }
        public int? MissionAttempt { get; set; }
        public bool? IsGamePlay { get; set; }
    }
}
