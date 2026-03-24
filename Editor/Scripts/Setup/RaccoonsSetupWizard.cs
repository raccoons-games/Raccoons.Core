using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Raccoons.Editor
{
    public class RaccoonsSetupWizard : EditorWindow
    {
        private const int PageWelcome = 0;
        private const int PageChecks = 1;
        private const int PageSetup = 2;

        private enum StepStatus { Pending, Success, Error }

        private int _page = PageWelcome;

        // Setup page state
        private bool _setupStarted;
        private StepStatus _addEntryPointStatus = StepStatus.Pending;
        private string _addEntryPointError;
        private StepStatus _addSceneEntryPointStatus = StepStatus.Pending;
        private string _addSceneEntryPointError;

        // Styles
        private GUIStyle _headerTitleStyle;
        private GUIStyle _headerSubtitleStyle;
        private GUIStyle _bodyLabelStyle;
        private GUIStyle _errorLabelStyle;
        private GUIStyle _okLabelStyle;
        private GUIStyle _pendingLabelStyle;
        private GUIStyle _primaryButtonStyle;
        private bool _stylesInitialized;

        [MenuItem("Raccoons/Setup Wizard")]
        public static void Open()
        {
            var window = GetWindow<RaccoonsSetupWizard>(true, "Raccoons Core Setup", true);
            window.minSize = new Vector2(560, 300);
            window.maxSize = new Vector2(560, 300);
            window.position = CenteredRect(560, 300);
            window.Show();
        }

        internal static void OpenOnFirstLaunch()
        {
            var window = CreateInstance<RaccoonsSetupWizard>();
            window.titleContent = new GUIContent("Raccoons Core Setup");
            window.minSize = new Vector2(560, 300);
            window.maxSize = new Vector2(560, 300);
            window.position = CenteredRect(560, 300);
            window.ShowUtility();
        }

        private static Rect CenteredRect(float w, float h)
        {
            var main = EditorGUIUtility.GetMainWindowPosition();
            return new Rect(main.x + (main.width - w) / 2f, main.y + (main.height - h) / 2f, w, h);
        }

        private void OnGUI()
        {
            EnsureStyles();

            switch (_page)
            {
                case PageWelcome: DrawWelcomePage(); break;
                case PageChecks:  DrawChecksPage();  break;
                case PageSetup:   DrawSetupPage();   break;
            }
        }

        // ── Welcome page ──────────────────────────────────────────────────────

        private void DrawWelcomePage()
        {
            DrawHeader("Welcome to Raccoons Core", "Let's make sure your project is set up correctly.");

            GUILayout.Space(16);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(24);
                EditorGUILayout.LabelField(
                    "This wizard will check that all required assets and configurations are in place for Raccoons Core to work properly.",
                    _bodyLabelStyle);
                GUILayout.Space(24);
            }

            GUILayout.Space(8);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(24);
                EditorGUILayout.LabelField("Click Setup to run the checks.", _bodyLabelStyle);
                GUILayout.Space(24);
            }

            GUILayout.Space(16);
            DrawSeparator();
            GUILayout.Space(8);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Setup", _primaryButtonStyle, GUILayout.Width(100), GUILayout.Height(28)))
                    _page = PageChecks;
                GUILayout.Space(16);
            }
        }

        // ── Checks page ───────────────────────────────────────────────────────

        private void DrawChecksPage()
        {
            DrawHeader("Project Checks", "Make sure all errors fixed before using package's features!");

            GUILayout.Space(16);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(24);
                DrawProjectContextCheck();
                GUILayout.Space(24);
            }

            GUILayout.Space(16);
            DrawSeparator();
            GUILayout.Space(8);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Continue", EditorStyles.miniButton, GUILayout.Width(80), GUILayout.Height(24)))
                    _page = PageSetup;
                GUILayout.Space(16);
            }
        }

        private void DrawProjectContextCheck()
        {
            var exists = ProjectContextExistsInResources();

            if (exists)
            {
                EditorGUILayout.LabelField("✔  ProjectContext.prefab found in Resources/", _okLabelStyle);
            }
            else
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(
                        "✖  No ProjectContext.prefab was found in Resources/. Create one?",
                        _errorLabelStyle, GUILayout.ExpandWidth(true));

                    GUILayout.Space(8);

                    if (GUILayout.Button("Yes", EditorStyles.miniButton, GUILayout.Width(36), GUILayout.Height(18)))
                    {
                        EditorApplication.ExecuteMenuItem("Edit/Zenject/Create Project Context");
                        Repaint();
                    }
                }
            }
        }

        // ── Setup page ────────────────────────────────────────────────────────

        private void DrawSetupPage()
        {
            DrawHeader("Project Setup", "Prepare your project's core entry points.");

            GUILayout.Space(14);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(24);
                EditorGUILayout.LabelField(
                    "The steps below will configure your project. Click Start to apply them automatically.",
                    _bodyLabelStyle);
                GUILayout.Space(24);
            }

            GUILayout.Space(12);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(24);
                using (new EditorGUILayout.VerticalScope())
                {
                    DrawStepRow(
                        "Add ProjectEntryPoint.prefab as a child of ProjectContext.prefab",
                        _addEntryPointStatus,
                        _addEntryPointError);

                    GUILayout.Space(6);

                    DrawStepRow(
                        "Add SceneEntryPoint.prefab to all scenes in build settings",
                        _addSceneEntryPointStatus,
                        _addSceneEntryPointError);
                }
                GUILayout.Space(24);
            }

            GUILayout.Space(16);
            DrawSeparator();
            GUILayout.Space(8);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (!_setupStarted)
                {
                    if (GUILayout.Button("Start", _primaryButtonStyle, GUILayout.Width(100), GUILayout.Height(28)))
                        RunSetup();
                }
                else
                {
                    if (GUILayout.Button("Close", EditorStyles.miniButton, GUILayout.Width(80), GUILayout.Height(24)))
                    {
                        RaccoonsSetupLauncher.MarkSetupCompleted();
                        Close();
                    }
                }

                GUILayout.Space(16);
            }
        }

        private void DrawStepRow(string label, StepStatus status, string errorMessage)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                string icon;
                GUIStyle style;

                switch (status)
                {
                    case StepStatus.Success:
                        icon = "✔";
                        style = _okLabelStyle;
                        break;
                    case StepStatus.Error:
                        icon = "✖";
                        style = _errorLabelStyle;
                        break;
                    default:
                        icon = "◦";
                        style = _pendingLabelStyle;
                        break;
                }

                EditorGUILayout.LabelField($"{icon}  {label}", style);
            }

            if (status == StepStatus.Error && !string.IsNullOrEmpty(errorMessage))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField(errorMessage, _errorLabelStyle);
                }
            }
        }

        // ── Setup logic ───────────────────────────────────────────────────────

        private void RunSetup()
        {
            _setupStarted = true;

            var error = StepAddEntryPointToProjectContext();
            _addEntryPointStatus = error == null ? StepStatus.Success : StepStatus.Error;
            _addEntryPointError = error;

            error = StepAddSceneEntryPointToScenes();
            _addSceneEntryPointStatus = error == null ? StepStatus.Success : StepStatus.Error;
            _addSceneEntryPointError = error;

            Repaint();
        }

        private static string StepAddEntryPointToProjectContext()
        {
            // Find ProjectContext.prefab in any Resources/ folder
            var contextGuids = AssetDatabase.FindAssets("ProjectContext t:Prefab");
            var contextPath = contextGuids
                .Select(g => AssetDatabase.GUIDToAssetPath(g).Replace("\\", "/"))
                .FirstOrDefault(p => p.Contains("/Resources/"));

            if (contextPath == null)
                return "ProjectContext.prefab not found in Resources/.";

            // Find ProjectEntryPoint.prefab (prefer the one in Raccoons.Core templates)
            var entryGuids = AssetDatabase.FindAssets("ProjectEntryPoint t:Prefab");
            var entryPath = entryGuids
                .Select(g => AssetDatabase.GUIDToAssetPath(g).Replace("\\", "/"))
                .FirstOrDefault(p => p.Contains("Raccoons") && p.Contains("Templates"));

            if (entryPath == null)
                return "ProjectEntryPoint.prefab not found in Raccoons.Core/Templates/.";

            var entryPointPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(entryPath);
            if (entryPointPrefab == null)
                return $"Failed to load ProjectEntryPoint.prefab at '{entryPath}'.";

            var root = PrefabUtility.LoadPrefabContents(contextPath);
            try
            {
                // Skip if already a child
                foreach (Transform child in root.transform)
                {
                    var src = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
                    if (src != null && AssetDatabase.GetAssetPath(src).Replace("\\", "/") == entryPath)
                        return null; // already present — treat as success
                }

                PrefabUtility.InstantiatePrefab(entryPointPrefab, root.transform);
                PrefabUtility.SaveAsPrefabAsset(root, contextPath);
                AssetDatabase.SaveAssets();
                return null;
            }
            catch (System.Exception e)
            {
                return e.Message;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static string StepAddSceneEntryPointToScenes()
        {
            var entryGuids = AssetDatabase.FindAssets("SceneEntryPoint t:Prefab");
            var entryPath = entryGuids
                .Select(g => AssetDatabase.GUIDToAssetPath(g).Replace("\\", "/"))
                .FirstOrDefault(p => p.Contains("Raccoons") && p.Contains("Templates"));

            if (entryPath == null)
                return "SceneEntryPoint.prefab not found in Raccoons.Core/Templates/.";

            var sceneEntryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(entryPath);
            if (sceneEntryPrefab == null)
                return $"Failed to load SceneEntryPoint.prefab at '{entryPath}'.";

            var buildScenes = EditorBuildSettings.scenes;
            if (buildScenes.Length == 0)
                return "No scenes found in build settings.";

            var errors = new List<string>();

            foreach (var buildScene in buildScenes)
            {
                try
                {
                    var existing = EditorSceneManager.GetSceneByPath(buildScene.path);
                    var wasLoaded = existing.isLoaded;
                    var scene = wasLoaded
                        ? existing
                        : EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Additive);

                    var alreadyPresent = scene.GetRootGameObjects().Any(root =>
                    {
                        var src = PrefabUtility.GetCorrespondingObjectFromSource(root);
                        return src != null &&
                               AssetDatabase.GetAssetPath(src).Replace("\\", "/") == entryPath;
                    });

                    if (!alreadyPresent)
                    {
                        PrefabUtility.InstantiatePrefab(sceneEntryPrefab, scene);
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                    }

                    if (!wasLoaded)
                        EditorSceneManager.CloseScene(scene, true);
                }
                catch (System.Exception e)
                {
                    errors.Add($"{System.IO.Path.GetFileName(buildScene.path)}: {e.Message}");
                }
            }

            return errors.Count > 0 ? string.Join("\n", errors) : null;
        }

        // ── Shared helpers ────────────────────────────────────────────────────

        private static bool ProjectContextExistsInResources()
        {
            var guids = AssetDatabase.FindAssets("ProjectContext t:Prefab");
            return guids.Any(g => AssetDatabase.GUIDToAssetPath(g).Replace("\\", "/").Contains("/Resources/"));
        }

        private void DrawHeader(string title, string subtitle)
        {
            var headerRect = GUILayoutUtility.GetRect(0, 64, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(headerRect, new Color(0.13f, 0.13f, 0.13f));

            GUI.Label(new Rect(headerRect.x + 16, headerRect.y + 10, headerRect.width - 32, 28),
                title, _headerTitleStyle);
            GUI.Label(new Rect(headerRect.x + 16, headerRect.y + 38, headerRect.width - 32, 18),
                subtitle, _headerSubtitleStyle);
        }

        private static void DrawSeparator()
        {
            var rect = GUILayoutUtility.GetRect(0, 1, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(0.25f, 0.25f, 0.25f));
        }

        // ── Style initialization ──────────────────────────────────────────────

        private void EnsureStyles()
        {
            if (_stylesInitialized && _headerTitleStyle != null) return;

            _headerTitleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 17,
                normal = { textColor = Color.white }
            };

            _headerSubtitleStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                normal = { textColor = new Color(0.65f, 0.65f, 0.65f) }
            };

            _bodyLabelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                wordWrap = true,
                normal = { textColor = new Color(0.8f, 0.8f, 0.8f) }
            };

            _errorLabelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                wordWrap = true,
                normal = { textColor = new Color(0.95f, 0.35f, 0.35f) }
            };

            _okLabelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                normal = { textColor = new Color(0.3f, 0.9f, 0.4f) }
            };

            _pendingLabelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }
            };

            _primaryButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.25f, 0.85f, 0.4f) },
                hover = { textColor = new Color(0.3f, 1f, 0.5f) }
            };

            _stylesInitialized = true;
        }
    }
}
