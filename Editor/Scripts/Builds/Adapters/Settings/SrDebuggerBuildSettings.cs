namespace Raccoons.Builds.Adapters.SRDebuggerAdapter
{
    [System.Serializable]
    public class SrDebuggerBuildSettings: BaseBuildAdapterSettings
    {
        public bool IsEnabled;
        public override void SetDefaultDevSettings()
        {
            IsEnabled = true;
        }
        public override void SetDefaultProdSettings()
        {
            IsEnabled = false;
        }
    }
}