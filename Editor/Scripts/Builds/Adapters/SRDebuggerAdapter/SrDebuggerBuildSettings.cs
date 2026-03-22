#if RACCOONS_INTEGRATION_SRDEBUGGER
namespace Raccoons.Builds.Adapters.SRDebugger
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
#endif