#if RACCOONS_INTEGRATION_SRDEBUGGER
using Raccoons.Builds.Adapters;
using SRDebugger;
using UnityEditor;
using UnityEngine;

namespace Raccoons.Builds.Adapters.SRDebugger
{
    public class SRDebuggerBuildAdapter : BaseBuildSettingsAdapter
    {
        public override BaseBuildAdapterSettings CreateDefaultSettings()
        {
            return new SrDebuggerBuildSettings();
        }

        public override void ApplySettings(AppConfiguration appConfiguration)
        {
            var srSettings = Settings.Instance;
            var adapterSettings = appConfiguration.GetSettings<SrDebuggerBuildSettings>();

            if (srSettings != null && adapterSettings != null)
            {
                if (adapterSettings.IsEnabled)
                {
                    srSettings.EnableTrigger = Settings.TriggerEnableModes.Enabled;
                    srSettings.TriggerBehaviour = Settings.TriggerBehaviours.TripleTap;
                }
                else
                {
                    srSettings.EnableTrigger = Settings.TriggerEnableModes.Off;
                }
                EditorUtility.SetDirty(srSettings);
                Debug.Log($"[SRDebuggerAdapter] Applied settings: IsEnabled={adapterSettings.IsEnabled}");
            }
        }

        public override bool i_IsAvailable()
        {
            // UPM install: dedicated assembly definition exists
            if (AssetDatabase.FindAssets("t:AssemblyDefinitionAsset StompyRobot.SRDebugger").Length > 0)
                return true;

            // .unitypackage install: scripts end up in Assembly-CSharp, check by type
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetType("SRDebugger.SROptions") != null)
                    return true;
            }

            return false;
        }
    }
}
#endif
