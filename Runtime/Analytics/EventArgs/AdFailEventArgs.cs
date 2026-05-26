namespace Raccoons.Analytics
{
    public class AdFailEventArgs
    {
        public string Network { get; set; }
        public AdErrorType Reason { get; set; }
        public int? Level { get; set; }
        public string Placement { get; set; }
    }
}
