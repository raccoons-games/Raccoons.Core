using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Raccoons.Builds
{
    public class GitTagPostBuildProcessor : IPostprocessBuildWithReport
    {
        public int callbackOrder => 100;
        
        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.result != BuildResult.Succeeded && report.summary.result != BuildResult.Unknown)
                return;
            
            if (report.summary.platform != BuildTarget.Android)
                return;
            
            if (!EditorUserBuildSettings.buildAppBundle)
                return;
            
            bool isDevelopment = report.summary.options.HasFlag(BuildOptions.Development);
            string buildType = isDevelopment ? "dev" : "prod";
            
            GitTagHelper.ShowGitTagDialogForBuild(buildType);
        }
    }
}
