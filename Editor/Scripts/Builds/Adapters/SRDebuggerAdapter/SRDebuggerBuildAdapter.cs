using SRDebugger;
using UnityEditor;
using UnityEngine;

namespace Raccoons.Builds.Adapters
{
    public class SRDebuggerBuildAdapter: BaseBuildSettingsAdapter
    {
        public override void ApplySettings(AppConfiguration appConfiguration)
        {
            var srSettings = Settings.Instance;
            var adapterSettings = appConfiguration.DebuggerBuildSettings;
        
            if (srSettings != null && adapterSettings != null)
            {
                if (adapterSettings.IsEnabled)
                {
                    srSettings.EnableTrigger = Settings.TriggerEnableModes.Enabled;
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
            return AssetDatabase.FindAssets("t:AssemblyDefinitionAsset StompyRobot.SRDebugger")
                .Length > 0;
        }
    }
}