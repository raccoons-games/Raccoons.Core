using System;
using System.Collections.Generic;
using System.Linq;
using Raccoons.Builds.Adapters;
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
        [SerializeReference] private List<BaseBuildAdapterSettings> adapterSettings = new List<BaseBuildAdapterSettings>();

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
            return adapterSettings ?? Enumerable.Empty<BaseBuildAdapterSettings>();
        }

        public T GetSettings<T>() where T : BaseBuildAdapterSettings
        {
            return adapterSettings?.OfType<T>().FirstOrDefault();
        }

        public BaseBuildAdapterSettings FindSettings(Type type)
        {
            return adapterSettings?.FirstOrDefault(s => s?.GetType() == type);
        }

        public void EnsureSettings(BaseBuildAdapterSettings settings)
        {
            if (adapterSettings == null)
                adapterSettings = new List<BaseBuildAdapterSettings>();

            if (FindSettings(settings.GetType()) == null)
                adapterSettings.Add(settings);
        }
#endif
    }
}