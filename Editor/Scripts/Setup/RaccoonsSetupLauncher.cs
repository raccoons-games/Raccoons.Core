using UnityEditor;
using UnityEngine;

namespace Raccoons.Editor
{
    [InitializeOnLoad]
    public static class RaccoonsSetupLauncher
    {
        private const string ShownThisSessionKey = "Raccoons.Core.Setup.ShownThisSession";

        private static string ProjectKey =>
            $"Raccoons.Core.{PlayerSettings.companyName}.{PlayerSettings.productName}.Setup";

        private static string CompletedKey => ProjectKey + ".Completed";
        private static string DismissedKey => ProjectKey + ".Dismissed";

        public static bool IsSetupCompleted => EditorPrefs.GetBool(CompletedKey, false);

        static RaccoonsSetupLauncher()
        {
            if (!ShouldAutoOpen()) return;

            if (RaccoonsDependencyInstaller.InstallationComplete)
                EditorApplication.delayCall += OpenWizardOnce;
            else
                RaccoonsDependencyInstaller.OnInstallationComplete += OnDepsReady;
        }

        // The static constructor runs on every domain reload (each recompile and each
        // Play mode entry), so the wizard opens by itself at most once per Editor session,
        // never in batch mode or while entering Play mode, and never again for a project
        // once it was completed or closed.
        private static bool ShouldAutoOpen()
        {
            if (Application.isBatchMode) return false;
            if (EditorApplication.isPlayingOrWillChangePlaymode) return false;
            if (SessionState.GetBool(ShownThisSessionKey, false)) return false;
            if (EditorPrefs.GetBool(CompletedKey, false)) return false;
            if (EditorPrefs.GetBool(DismissedKey, false)) return false;
            return true;
        }

        private static void OnDepsReady()
        {
            RaccoonsDependencyInstaller.OnInstallationComplete -= OnDepsReady;
            if (ShouldAutoOpen())
                EditorApplication.delayCall += OpenWizardOnce;
        }

        private static void OpenWizardOnce()
        {
            EditorApplication.delayCall -= OpenWizardOnce;
            if (!ShouldAutoOpen()) return;

            SessionState.SetBool(ShownThisSessionKey, true);
            RaccoonsSetupWizard.OpenOnFirstLaunch();
        }

        public static void MarkSetupCompleted()
        {
            EditorPrefs.SetBool(CompletedKey, true);
        }

        /// <summary>
        /// Stops the wizard from opening by itself for this project. It stays available
        /// from the Raccoons/Setup Wizard menu.
        /// </summary>
        public static void MarkSetupDismissed()
        {
            EditorPrefs.SetBool(DismissedKey, true);
        }

        /// <summary>
        /// Clears the completed and dismissed flags so the wizard will show again next editor launch.
        /// Useful for testing or after re-importing the package.
        /// </summary>
        public static void ResetSetupCompleted()
        {
            EditorPrefs.DeleteKey(CompletedKey);
            EditorPrefs.DeleteKey(DismissedKey);
            SessionState.EraseBool(ShownThisSessionKey);
        }
    }
}
