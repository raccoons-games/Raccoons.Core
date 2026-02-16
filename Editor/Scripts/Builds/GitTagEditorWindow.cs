using UnityEditor;
using UnityEngine;

namespace Raccoons.Builds
{
    public class GitTagEditorWindow : EditorWindow
    {
        private string _tagName = "";
        private string _currentBranch = "";
        
        public static void ShowWindow(string suggestedTag, string currentBranch)
        {
            var window = GetWindow<GitTagEditorWindow>(true, "Create Git Tag", true);
            window.minSize = new Vector2(400, 180);
            window.maxSize = new Vector2(400, 180);
            window._tagName = suggestedTag;
            window._currentBranch = currentBranch;
            window.ShowUtility();
        }
        
        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            
            EditorGUILayout.LabelField("Create Git Tag (Local Only)", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            EditorGUILayout.LabelField($"Current Branch: {_currentBranch}", EditorStyles.helpBox);
            EditorGUILayout.Space(10);
            
            EditorGUILayout.LabelField("Tag Name:", EditorStyles.boldLabel);
            _tagName = EditorGUILayout.TextField(_tagName);
            
            EditorGUILayout.Space(15);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Cancel", GUILayout.Width(100), GUILayout.Height(30)))
            {
                Close();
            }
            
            GUI.enabled = !string.IsNullOrWhiteSpace(_tagName);
            if (GUILayout.Button("Create Tag", GUILayout.Width(100), GUILayout.Height(30)))
            {
                CreateTag();
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
        }
        
        private void CreateTag()
        {
            if (string.IsNullOrWhiteSpace(_tagName))
            {
                EditorUtility.DisplayDialog("Invalid Tag Name", "Tag name cannot be empty.", "OK");
                return;
            }
            
            if (GitTagHelper.TagExists(_tagName))
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "Tag Already Exists",
                    $"Tag '{_tagName}' already exists locally. Do you want to delete and recreate it?",
                    "Yes, Recreate",
                    "Cancel"
                );
                
                if (!overwrite)
                    return;
                
                GitTagHelper.ExecuteGitCommand($"git tag -d \"{_tagName}\"");
            }
            
            GitTagHelper.CreateGitTag(_tagName, _tagName);
            
            Close();
        }
    }
}
