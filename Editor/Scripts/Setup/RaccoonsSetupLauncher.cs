using UnityEditor;
using UnityEngine;

namespace Raccoons.Editor
{
    [InitializeOnLoad]
    public static class RaccoonsSetupLauncher
    {
        private static string CompletedKey =>
            $"Raccoons.Core.{PlayerSettings.companyName}.{PlayerSettings.productName}.Setup.Completed";

        static RaccoonsSetupLauncher()
        {
            if (EditorPrefs.GetBool(CompletedKey, false)) return;

            EditorApplication.delayCall += OpenWizardOnce;
        }

        private static void OpenWizardOnce()
        {
            EditorApplication.delayCall -= OpenWizardOnce;
            RaccoonsSetupWizard.OpenOnFirstLaunch();
        }

        public static void MarkSetupCompleted()
        {
            EditorPrefs.SetBool(CompletedKey, true);
        }

        /// <summary>
        /// Clears the completed flag so the wizard will show again next editor launch.
        /// Useful for testing or after re-importing the package.
        /// </summary>
        public static void ResetSetupCompleted()
        {
            EditorPrefs.DeleteKey(CompletedKey);
        }
    }
}
