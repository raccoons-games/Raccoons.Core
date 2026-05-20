using System;
using System.Collections.Generic;
using System.IO;
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

        private enum StepStatus { Pending, InProgress, Success, Skipped, Error }

        // ── Serialized state (survives domain reload) ────────────────────────

        [SerializeField] private int _page = PageWelcome;
        [SerializeField] private bool _setupStarted;
        [SerializeField] private int _addEntryStatusInt = (int)StepStatus.Pending;
        [SerializeField] private string _addEntryError;
        [SerializeField] private int _addSceneStatusInt = (int)StepStatus.Pending;
        [SerializeField] private string _addSceneError;

        private StepStatus AddEntryStatus
        {
            get => (StepStatus)_addEntryStatusInt;
            set => _addEntryStatusInt = (int)value;
        }

        private StepStatus AddSceneStatus
        {
            get => (StepStatus)_addSceneStatusInt;
            set => _addSceneStatusInt = (int)value;
        }

        // ── Styles ───────────────────────────────────────────────────────────

        private GUIStyle _headerTitleStyle;
        private GUIStyle _headerSubtitleStyle;
        private GUIStyle _bodyLabelStyle;
        private GUIStyle _errorLabelStyle;
        private GUIStyle _okLabelStyle;
        private GUIStyle _pendingLabelStyle;
        private GUIStyle _inProgressLabelStyle;
        private GUIStyle _primaryButtonStyle;
        private bool _stylesInitialized;

        // ── Window open ───────────────────────────────────────────────────────

        [MenuItem("Raccoons/Setup Wizard")]
        public static void Open()
        {
            var w = GetWindow<RaccoonsSetupWizard>(true, "Raccoons Core Setup", true);
            ApplyWindowSize(w);
            w.Show();
        }

        internal static void OpenOnFirstLaunch()
        {
            var w = CreateInstance<RaccoonsSetupWizard>();
            w.titleContent = new GUIContent("Raccoons Core Setup");
            ApplyWindowSize(w);
            w.ShowUtility();
        }

        private static void ApplyWindowSize(EditorWindow w)
        {
            w.minSize = new Vector2(560, 420);
            w.maxSize = new Vector2(560, 420);
            w.position = CenteredRect(560, 420);
        }

        private static Rect CenteredRect(float w, float h)
        {
            var main = EditorGUIUtility.GetMainWindowPosition();
            return new Rect(main.x + (main.width - w) / 2f, main.y + (main.height - h) / 2f, w, h);
        }

        // ── OnGUI ─────────────────────────────────────────────────────────────

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
                    "All required dependencies are installed automatically via the Unity Package Manager. This wizard will configure your project entry points.",
                    _bodyLabelStyle);
                GUILayout.Space(24);
            }

            GUILayout.FlexibleSpace();
            DrawSeparator();
            GUILayout.Space(8);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Begin Setup", _primaryButtonStyle, GUILayout.Width(120), GUILayout.Height(28)))
                    _page = PageChecks;
                GUILayout.Space(16);
            }
            GUILayout.Space(8);
        }

        // ── Checks page ───────────────────────────────────────────────────────

        private void DrawChecksPage()
        {
            DrawHeader("Project Checks", "Make sure all errors are fixed before using the package.");
            GUILayout.Space(16);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(24);
                DrawProjectContextCheck();
                GUILayout.Space(24);
            }

            GUILayout.FlexibleSpace();
            DrawSeparator();
            GUILayout.Space(8);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                using (new EditorGUI.DisabledScope(!AllChecksPass()))
                {
                    if (GUILayout.Button("Continue \u2192", EditorStyles.miniButton, GUILayout.Width(90), GUILayout.Height(24)))
                        _page = PageSetup;
                }
                GUILayout.Space(16);
            }
            GUILayout.Space(8);
        }

        private void DrawProjectContextCheck()
        {
            bool exists = ProjectContextExistsInResources();
            if (exists)
            {
                EditorGUILayout.LabelField("\u2714  ProjectContext.prefab found in Resources/", _okLabelStyle);
            }
            else
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(
                        "\u2716  No ProjectContext.prefab found in Resources/. Create one?",
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
                    DrawStepRow("Add ProjectEntryPoint.prefab as a child of ProjectContext.prefab",
                        AddEntryStatus, _addEntryError);
                    GUILayout.Space(6);
                    DrawStepRow("Add SceneEntryPoint.prefab to all scenes in build settings",
                        AddSceneStatus, _addSceneError);
                }
                GUILayout.Space(24);
            }

            GUILayout.FlexibleSpace();
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
            GUILayout.Space(8);
        }

        // ── Setup logic ───────────────────────────────────────────────────────

        private void RunSetup()
        {
            _setupStarted = true;

            string error = StepAddEntryPointToProjectContext();
            AddEntryStatus = error == null ? StepStatus.Success : StepStatus.Error;
            _addEntryError = error;

            error = StepAddSceneEntryPointToScenes();
            AddSceneStatus = error == null ? StepStatus.Success : StepStatus.Error;
            _addSceneError = error;

            Repaint();
        }

        private static string StepAddEntryPointToProjectContext()
        {
            var contextGuids = AssetDatabase.FindAssets("ProjectContext t:Prefab");
            var contextPath = contextGuids
                .Select(g => AssetDatabase.GUIDToAssetPath(g).Replace("\\", "/"))
                .FirstOrDefault(p => p.Contains("/Resources/"));
            if (contextPath == null) return "ProjectContext.prefab not found in Resources/.";

            var entryGuids = AssetDatabase.FindAssets("ProjectEntryPoint t:Prefab");
            var entryPath = entryGuids
                .Select(g => AssetDatabase.GUIDToAssetPath(g).Replace("\\", "/"))
                .FirstOrDefault(p => p.IndexOf("Raccoons", StringComparison.OrdinalIgnoreCase) >= 0
                                     && p.Contains("Templates"));
            if (entryPath == null) return "ProjectEntryPoint.prefab not found in Raccoons.Core/Templates/.";

            var entryPointPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(entryPath);
            if (entryPointPrefab == null) return $"Failed to load ProjectEntryPoint.prefab at '{entryPath}'.";

            var root = PrefabUtility.LoadPrefabContents(contextPath);
            try
            {
                foreach (Transform child in root.transform)
                {
                    var src = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
                    if (src != null && AssetDatabase.GetAssetPath(src).Replace("\\", "/") == entryPath)
                        return null;
                }
                PrefabUtility.InstantiatePrefab(entryPointPrefab, root.transform);
                PrefabUtility.SaveAsPrefabAsset(root, contextPath);
                AssetDatabase.SaveAssets();
                return null;
            }
            catch (Exception e) { return e.Message; }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static string StepAddSceneEntryPointToScenes()
        {
            var entryGuids = AssetDatabase.FindAssets("SceneEntryPoint t:Prefab");
            var entryPath = entryGuids
                .Select(g => AssetDatabase.GUIDToAssetPath(g).Replace("\\", "/"))
                .FirstOrDefault(p => p.IndexOf("Raccoons", StringComparison.OrdinalIgnoreCase) >= 0
                                     && p.Contains("Templates"));
            if (entryPath == null) return "SceneEntryPoint.prefab not found in Raccoons.Core/Templates/.";

            var sceneEntryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(entryPath);
            if (sceneEntryPrefab == null) return $"Failed to load SceneEntryPoint.prefab at '{entryPath}'.";

            var buildScenes = EditorBuildSettings.scenes;
            if (buildScenes.Length == 0) return "No scenes found in build settings.";

            var errors = new List<string>();
            foreach (var buildScene in buildScenes)
            {
                try
                {
                    var existing = EditorSceneManager.GetSceneByPath(buildScene.path);
                    bool wasLoaded = existing.isLoaded;
                    var scene = wasLoaded
                        ? existing
                        : EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Additive);

                    bool alreadyPresent = scene.GetRootGameObjects().Any(r =>
                    {
                        var src = PrefabUtility.GetCorrespondingObjectFromSource(r);
                        return src != null && AssetDatabase.GetAssetPath(src).Replace("\\", "/") == entryPath;
                    });

                    if (!alreadyPresent)
                    {
                        PrefabUtility.InstantiatePrefab(sceneEntryPrefab, scene);
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                    }

                    if (!wasLoaded) EditorSceneManager.CloseScene(scene, true);
                }
                catch (Exception e)
                {
                    errors.Add($"{Path.GetFileName(buildScene.path)}: {e.Message}");
                }
            }

            return errors.Count > 0 ? string.Join("\n", errors) : null;
        }

        // ── Shared helpers ────────────────────────────────────────────────────

        private static bool AllChecksPass() => ProjectContextExistsInResources();

        private static bool ProjectContextExistsInResources()
        {
            var guids = AssetDatabase.FindAssets("ProjectContext t:Prefab");
            return guids.Any(g => AssetDatabase.GUIDToAssetPath(g).Replace("\\", "/").Contains("/Resources/"));
        }

        private void DrawStepRow(string label, StepStatus status, string errorMessage)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                string icon;
                GUIStyle style;
                switch (status)
                {
                    case StepStatus.Success:    icon = "\u2714"; style = _okLabelStyle;         break;
                    case StepStatus.Skipped:    icon = "\u21b7"; style = _okLabelStyle;         break;
                    case StepStatus.Error:      icon = "\u2716"; style = _errorLabelStyle;      break;
                    case StepStatus.InProgress: icon = "\u29d6"; style = _inProgressLabelStyle; break;
                    default:                    icon = "\u25e6"; style = _pendingLabelStyle;     break;
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

        private void DrawHeader(string title, string subtitle)
        {
            var r = GUILayoutUtility.GetRect(0, 64, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(r, new Color(0.13f, 0.13f, 0.13f));
            GUI.Label(new Rect(r.x + 16, r.y + 10, r.width - 32, 28), title, _headerTitleStyle);
            GUI.Label(new Rect(r.x + 16, r.y + 38, r.width - 32, 18), subtitle, _headerSubtitleStyle);
        }

        private static void DrawSeparator()
        {
            var r = GUILayoutUtility.GetRect(0, 1, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(r, new Color(0.25f, 0.25f, 0.25f));
        }

        private void EnsureStyles()
        {
            if (_stylesInitialized && _headerTitleStyle != null) return;

            _headerTitleStyle = new GUIStyle(EditorStyles.boldLabel)
                { fontSize = 17, normal = { textColor = Color.white } };

            _headerSubtitleStyle = new GUIStyle(EditorStyles.label)
                { fontSize = 11, normal = { textColor = new Color(0.65f, 0.65f, 0.65f) } };

            _bodyLabelStyle = new GUIStyle(EditorStyles.label)
                { fontSize = 12, wordWrap = true, normal = { textColor = new Color(0.8f, 0.8f, 0.8f) } };

            _errorLabelStyle = new GUIStyle(EditorStyles.label)
                { fontSize = 11, wordWrap = true, normal = { textColor = new Color(0.95f, 0.35f, 0.35f) } };

            _okLabelStyle = new GUIStyle(EditorStyles.label)
                { fontSize = 11, normal = { textColor = new Color(0.3f, 0.9f, 0.4f) } };

            _pendingLabelStyle = new GUIStyle(EditorStyles.label)
                { fontSize = 11, normal = { textColor = new Color(0.6f, 0.6f, 0.6f) } };

            _inProgressLabelStyle = new GUIStyle(EditorStyles.label)
                { fontSize = 11, normal = { textColor = new Color(0.9f, 0.8f, 0.2f) } };

            _primaryButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fontSize   = 13,
                fontStyle  = FontStyle.Bold,
                normal     = { textColor = new Color(0.25f, 0.85f, 0.4f) },
                hover      = { textColor = new Color(0.3f, 1f, 0.5f) }
            };

            _stylesInitialized = true;
        }
    }
}
