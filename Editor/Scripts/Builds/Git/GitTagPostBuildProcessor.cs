using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Raccoons.Builds
{
    public class GitTagPostBuildProcessor : IPostprocessBuildWithReport
    {
        public int callbackOrder => 100;
        
        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.result == BuildResult.Failed || report.summary.result == BuildResult.Cancelled)
                return;
            
            if (report.summary.platform != BuildTarget.Android)
                return;
            
            if (!EditorUserBuildSettings.buildAppBundle)
                return;
            
            
            GitTagHelper.ShowGitTagDialogForBuild();
        }
    }
}
