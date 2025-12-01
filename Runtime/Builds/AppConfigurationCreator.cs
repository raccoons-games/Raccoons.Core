#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Raccoons.Builds
{
    [InitializeOnLoad]
    public static class AppConfigurationCreator
    {
        static AppConfigurationCreator()
        {
            const string assetPath = "Assets/Resources/AppConfiguration.asset";

            var config = AssetDatabase.LoadAssetAtPath<AppConfiguration>(assetPath);
            if (config == null)
            {
                // Ensure Resources folder exists
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                    AssetDatabase.CreateFolder("Assets", "Resources");

                var asset = ScriptableObject.CreateInstance<AppConfiguration>();
                AssetDatabase.CreateAsset(asset, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }
}
#endif