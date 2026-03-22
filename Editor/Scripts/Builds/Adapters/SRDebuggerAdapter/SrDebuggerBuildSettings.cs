using Raccoons.Builds.Adapters;

namespace Raccoons.Builds.Adapters.SRDebugger
{
    [AdapterDisplayName("SR Debugger")]
    [System.Serializable]
    public class SrDebuggerBuildSettings : BaseBuildAdapterSettings
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
