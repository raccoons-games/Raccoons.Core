namespace Raccoons.Analytics
{
    public class MissionStepEventArgs : MissionEventArgs
    {
        public Reward Reward { get; set; }
        public string StepName { get; set; }
    }
}
