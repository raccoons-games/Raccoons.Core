using System.Collections.Generic;
using Raccoons.Builds.Adapters;
#if RACCOONS_INTEGRATION_SRDEBUGGER
using Raccoons.Builds.Adapters.SRDebugger;
#endif
using UnityEngine;

namespace Raccoons.Builds
{
    public class AppConfiguration : ScriptableObject
    {
        private const string ResourcePath = "AppConfiguration";
#if UNITY_EDITOR
        private const string AssetPath = "Assets/Resources/AppConfiguration.asset";
#endif

        [Header("Build Modes")]
        [SerializeField] private AppMode editorAppMode = AppMode.Dev;
        [SerializeField] private AppMode developmentBuildAppMode = AppMode.Dev;
        [SerializeField] private AppMode standardBuildAppMode = AppMode.Prod;
        [SerializeField] private bool activateDebugObjectsInProd;

        [Header("Adapter Settings")]
#if RACCOONS_INTEGRATION_SRDEBUGGER
        [SerializeField]
        private SrDebuggerBuildSettings debuggerBuildSettings;

        public SrDebuggerBuildSettings DebuggerBuildSettings => debuggerBuildSettings;
#endif
        
        
        public AppMode EditorAppMode => editorAppMode;
        public AppMode DevelopmentBuildAppMode => developmentBuildAppMode;
        public AppMode StandardBuildAppMode => standardBuildAppMode;
        public bool ActivateDebugObjectsInProd => activateDebugObjectsInProd;

        private static AppConfiguration instance;
        
        public static AppConfiguration Get()
        {
            if (instance == null)
                instance = Resources.Load<AppConfiguration>(ResourcePath);

            return instance;
        }
        
        public static AppMode GetMode()
        {
            if (instance == null)
                instance = Resources.Load<AppConfiguration>(ResourcePath);

            if (Application.isEditor)
                return instance.EditorAppMode;

            if (Debug.isDebugBuild)
                return instance.DevelopmentBuildAppMode;

            return instance.StandardBuildAppMode;
        }

        public static bool IsDev()
        {
            return GetMode() == AppMode.Dev;
        }

        public static bool IsDebugUIVisible()
        {
            return GetMode() == AppMode.Dev || Get().ActivateDebugObjectsInProd;
        }

#if UNITY_EDITOR
        public IEnumerable<BaseBuildAdapterSettings> GetAllAdapterSettings()
        {
#if RACCOONS_INTEGRATION_SRDEBUGGER
            if (debuggerBuildSettings != null)
                yield return debuggerBuildSettings;
#else
            yield break;
#endif
        }
#endif
    }
}