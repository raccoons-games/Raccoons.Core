using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace Raccoons.Builds
{
    public static class GitTagHelper
    {
        private static readonly string[] AllowedBranches = { "develop", "dev" };
        
        public static void ShowGitTagDialog()
        {
            string currentBranch = GetCurrentBranch();
            
            if (!IsAllowedBranch(currentBranch))
            {
                EditorUtility.DisplayDialog(
                    "Git Tag Not Allowed",
                    $"Git tags can only be created from 'develop' or 'dev' branches.\n\nCurrent branch: {currentBranch}",
                    "OK"
                );
                return;
            }
            
            string version = PlayerSettings.bundleVersion;
            int bundleCode = PlayerSettings.Android.bundleVersionCode;
            string suggestedTag = $"v{version}-{bundleCode}";
            
            ShowEditableTagDialog(suggestedTag, currentBranch);
        }
        
        public static void ShowGitTagDialogForBuild()
        {
            string currentBranch = GetCurrentBranch();
            
            if (!IsAllowedBranch(currentBranch))
            {
                UnityEngine.Debug.Log($"[GitTag] Skipping tag creation - not on allowed branch (current: {currentBranch})");
                return;
            }
            
            if (!IsGitRepository())
            {
                UnityEngine.Debug.LogWarning("[GitTag] Not in a git repository. Skipping tag creation.");
                return;
            }
            
            string version = PlayerSettings.bundleVersion;
            int bundleCode = PlayerSettings.Android.bundleVersionCode;
            string suggestedTag = $"v{version}-{bundleCode}";
            
            if (TagExists(suggestedTag))
            {
                UnityEngine.Debug.LogWarning($"[GitTag] Tag '{suggestedTag}' already exists. Skipping.");
                return;
            }
            
            ShowEditableTagDialog(suggestedTag, currentBranch);
        }
        
        private static void ShowEditableTagDialog(string suggestedTag, string currentBranch)
        {
            GitTagEditorWindow.ShowWindow(suggestedTag, currentBranch);
        }
        
        private static bool IsAllowedBranch(string branchName)
        {
            if (string.IsNullOrEmpty(branchName))
                return false;
            
            branchName = branchName.ToLower().Trim();
            
            foreach (var allowedBranch in AllowedBranches)
            {
                if (branchName == allowedBranch || branchName.StartsWith(allowedBranch + "/"))
                    return true;
            }
            
            return false;
        }
        
        public static bool IsGitRepository()
        {
            string result = ExecuteGitCommand("git rev-parse --git-dir");
            return !string.IsNullOrEmpty(result) && !result.Contains("fatal");
        }
        
        public static bool TagExists(string tagName)
        {
            string result = ExecuteGitCommand($"git tag -l \"{tagName}\"");
            return !string.IsNullOrEmpty(result) && result.Trim() == tagName;
        }
        
        public static string GetCurrentBranch()
        {
            string result = ExecuteGitCommand("git rev-parse --abbrev-ref HEAD");
            return result?.Trim() ?? "unknown";
        }
        
        public static void CreateGitTag(string tagName, string message)
        {
            string createTagCommand = $"git tag -a \"{tagName}\" -m \"{message}\"";
            string result = ExecuteGitCommand(createTagCommand);
            
            if (string.IsNullOrEmpty(result))
            {
                UnityEngine.Debug.Log($"[GitTag] Created tag: {tagName}");
                
                // Push the tag to remote
                string pushCommand = $"git push origin {tagName}";
                string pushResult = ExecuteGitCommand(pushCommand);
                
                if (string.IsNullOrEmpty(pushResult))
                {
                    UnityEngine.Debug.Log($"[GitTag] Pushed tag to remote: {tagName}");
                    EditorUtility.DisplayDialog(
                        "Git Tag Created & Pushed",
                        $"Tag '{tagName}' created and pushed successfully!",
                        "OK"
                    );
                }
                else
                {
                    UnityEngine.Debug.LogError($"[GitTag] Failed to push tag: {pushResult}");
                    EditorUtility.DisplayDialog(
                        "Push Failed",
                        $"Tag created locally but push failed:\n\n{pushResult}\n\nYou can push manually with:\ngit push origin {tagName}",
                        "OK"
                    );
                }
            }
            else
            {
                UnityEngine.Debug.LogError($"[GitTag] Failed to create tag: {result}");
                EditorUtility.DisplayDialog(
                    "Tag Creation Failed",
                    $"Failed to create tag:\n{result}",
                    "OK"
                );
            }
        }
        
        public static string ExecuteGitCommand(string command)
        {
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = command.Replace("git ", ""),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Application.dataPath.Replace("/Assets", "")
                };
                
                using (Process process = Process.Start(processInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                    
                    if (process.ExitCode != 0)
                    {
                        return error;
                    }
                    
                    return output;
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"[GitTag] Git command failed: {e.Message}");
                return e.Message;
            }
        }
    }
}
