using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Raccoons.Editor
{
    public class RaccoonsSetupWizard : EditorWindow
    {
        private const int PageWelcome = 0;
        private const int PageDependencies = 1;
        private const int PageChecks = 2;
        private const int PageSetup = 3;

        private enum StepStatus { Pending, InProgress, Success, Skipped, Error }
        private enum DepsPhase { Idle, Listing, Installing, InstallNewtonsoft, Done }

        // ── Package definitions ──────────────────────────────────────────────

        private class PackageDef
        {
            public string DisplayName;
            public string PackageId;   // UPM package name; null for NuGet
            public string GitUrl;      // null for NuGet packages
            public bool IsNuGet;
        }

        private static readonly PackageDef[] Packages =
        {
            new PackageDef
            {
                DisplayName = "UniTask",
                PackageId   = "com.cysharp.unitask",
                GitUrl      = "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask"
            },
            new PackageDef
            {
                DisplayName = "Zenject (Extenject)",
                PackageId   = "com.svermeulen.extenject",
                GitUrl      = "https://github.com/Mathijs-Bakker/Extenject.git?path=UnityProject/Assets/Plugins/Zenject/Source"
            },
            new PackageDef
            {
                DisplayName = "NuGet Package Manager",
                PackageId   = "com.github-glitchenco.nugetforunity",
                GitUrl      = "https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity"
            },
            new PackageDef
            {
                DisplayName = "Newtonsoft.Json",
                IsNuGet     = true
            },
        };

        // ── Serialized state (survives domain reload) ────────────────────────

        [SerializeField] private int _page = PageWelcome;
        [SerializeField] private int _depsPhaseInt = (int)DepsPhase.Idle;
        [SerializeField] private int[] _depsStatusInt;
        [SerializeField] private string[] _depsErrors;
        [SerializeField] private int _currentPkg = -1;
        [SerializeField] private float _progress;
        [SerializeField] private string _progressMessage = "";
        [SerializeField] private bool _hasListed;
        [SerializeField] private bool _setupStarted;
        [SerializeField] private int _addEntryStatusInt = (int)StepStatus.Pending;
        [SerializeField] private string _addEntryError;
        [SerializeField] private int _addSceneStatusInt = (int)StepStatus.Pending;
        [SerializeField] private string _addSceneError;

        // ── Non-serialized runtime state ─────────────────────────────────────

        private DepsPhase DepsPhaseState
        {
            get => (DepsPhase)_depsPhaseInt;
            set => _depsPhaseInt = (int)value;
        }

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

        private ListRequest _listRequest;
        private AddRequest _addRequest;

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

        internal static void OpenAtDependencies()
        {
            var existing = Resources.FindObjectsOfTypeAll<RaccoonsSetupWizard>().FirstOrDefault();
            if (existing != null)
            {
                existing._page = PageDependencies;
                existing.DepsPhaseState = DepsPhase.InstallNewtonsoft;
                existing.StartPolling();
                existing.Repaint();
                return;
            }

            var w = CreateInstance<RaccoonsSetupWizard>();
            w.titleContent = new GUIContent("Raccoons Core Setup");
            ApplyWindowSize(w);
            w._page = PageDependencies;
            w._depsPhaseInt = (int)DepsPhase.InstallNewtonsoft;
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

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void OnEnable()
        {
            EnsureStatusArrays();
            RecoverFromDomainReload();
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnUpdate;
        }

        private void EnsureStatusArrays()
        {
            if (_depsStatusInt == null || _depsStatusInt.Length != Packages.Length)
            {
                _depsStatusInt = new int[Packages.Length];
                _depsErrors = new string[Packages.Length];
                for (int i = 0; i < _depsStatusInt.Length; i++)
                    _depsStatusInt[i] = (int)StepStatus.Pending;
            }

            if (_depsErrors == null || _depsErrors.Length != Packages.Length)
                _depsErrors = new string[Packages.Length];
        }

        private void RecoverFromDomainReload()
        {
            var phase = DepsPhaseState;

            // If we were installing a UPM package, re-list to see if it succeeded
            if (phase == DepsPhase.Installing && _currentPkg >= 0 && _currentPkg < Packages.Length)
            {
                DepsPhaseState = DepsPhase.Listing;
                _listRequest = Client.List();
                StartPolling();
                return;
            }

            // If we were waiting to install Newtonsoft via NuGet
            if (phase == DepsPhase.InstallNewtonsoft)
            {
                StartPolling();
                return;
            }

            if (phase != DepsPhase.Idle && phase != DepsPhase.Done)
                StartPolling();
        }

        private void StartPolling()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
        }

        // ── OnGUI ─────────────────────────────────────────────────────────────

        private void OnGUI()
        {
            EnsureStyles();
            switch (_page)
            {
                case PageWelcome:      DrawWelcomePage();      break;
                case PageDependencies: DrawDependenciesPage(); break;
                case PageChecks:       DrawChecksPage();       break;
                case PageSetup:        DrawSetupPage();        break;
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
                    "This wizard will install required dependencies (UniTask, Zenject, NuGet, Newtonsoft.Json) and configure your project entry points.",
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
                    _page = PageDependencies;
                GUILayout.Space(16);
            }
            GUILayout.Space(8);
        }

        // ── Dependencies page ─────────────────────────────────────────────────

        private void DrawDependenciesPage()
        {
            // Auto-start check when entering this page for the first time
            if (DepsPhaseState == DepsPhase.Idle && !_hasListed)
                StartAutoCheck();

            DrawHeader("Install Dependencies", "Packages are installed one by one in order.");
            GUILayout.Space(12);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(24);
                using (new EditorGUILayout.VerticalScope())
                {
                    for (int i = 0; i < Packages.Length; i++)
                    {
                        DrawStepRow(Packages[i].DisplayName, (StepStatus)_depsStatusInt[i], _depsErrors[i]);
                        GUILayout.Space(4);
                    }
                }
                GUILayout.Space(24);
            }

            GUILayout.Space(12);

            if (DepsPhaseState != DepsPhase.Idle)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(24);
                    var r = GUILayoutUtility.GetRect(0, 22, GUILayout.ExpandWidth(true));
                    EditorGUI.ProgressBar(r, _progress, _progressMessage);
                    GUILayout.Space(24);
                }
                GUILayout.Space(8);
            }

            GUILayout.FlexibleSpace();
            DrawSeparator();
            GUILayout.Space(8);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                bool settled = AllDepsSettled();

                if (DepsPhaseState == DepsPhase.Done || (settled && _hasListed))
                {
                    if (GUILayout.Button("Continue \u2192", _primaryButtonStyle, GUILayout.Width(120), GUILayout.Height(28)))
                        _page = PageChecks;
                }
                else if (!_hasListed || DepsPhaseState == DepsPhase.Listing)
                {
                    using (new EditorGUI.DisabledScope(true))
                        GUILayout.Button("Checking\u2026", EditorStyles.miniButton, GUILayout.Width(120), GUILayout.Height(24));
                }
                else if (settled)
                {
                    // All already installed
                    if (GUILayout.Button("Continue \u2192", _primaryButtonStyle, GUILayout.Width(120), GUILayout.Height(28)))
                        _page = PageChecks;
                }
                else
                {
                    bool isInstalling = DepsPhaseState == DepsPhase.Installing
                                     || DepsPhaseState == DepsPhase.InstallNewtonsoft;
                    if (isInstalling)
                    {
                        using (new EditorGUI.DisabledScope(true))
                            GUILayout.Button("Installing\u2026", EditorStyles.miniButton, GUILayout.Width(120), GUILayout.Height(24));
                    }
                    else
                    {
                        if (GUILayout.Button("Install Missing", _primaryButtonStyle, GUILayout.Width(130), GUILayout.Height(28)))
                            StartInstallation();
                    }
                }

                GUILayout.Space(16);
            }
            GUILayout.Space(8);
        }

        private bool AllDepsSettled()
        {
            if (_depsStatusInt == null) return false;
            return _depsStatusInt.All(s =>
            {
                var status = (StepStatus)s;
                return status == StepStatus.Success || status == StepStatus.Skipped || status == StepStatus.Error;
            });
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

        // ── Installation logic ────────────────────────────────────────────────

        private void StartAutoCheck()
        {
            DepsPhaseState = DepsPhase.Listing;
            _progressMessage = "Checking installed packages\u2026";
            _progress = 0f;
            _listRequest = Client.List();
            StartPolling();
        }

        private void StartInstallation()
        {
            _hasListed = false;
            DepsPhaseState = DepsPhase.Listing;
            _progressMessage = "Checking installed packages\u2026";
            _progress = 0f;
            _listRequest = Client.List();
            StartPolling();
        }

        private void OnUpdate()
        {
            switch (DepsPhaseState)
            {
                case DepsPhase.Listing:           UpdateListing();           break;
                case DepsPhase.Installing:        UpdateInstalling();        break;
                case DepsPhase.InstallNewtonsoft: UpdateInstallNewtonsoft(); break;
            }
            Repaint();
        }

        private void UpdateListing()
        {
            if (_listRequest == null || !_listRequest.IsCompleted) return;

            var installed = new HashSet<string>();
            if (_listRequest.Status == StatusCode.Success)
                foreach (var p in _listRequest.Result)
                    installed.Add(p.name);

            if (!_hasListed)
            {
                // First listing: set status from scratch
                _hasListed = true;
                for (int i = 0; i < Packages.Length; i++)
                {
                    bool isInstalled = Packages[i].IsNuGet
                        ? IsNewtonsoftInstalled()
                        : installed.Contains(Packages[i].PackageId);
                    _depsStatusInt[i] = isInstalled ? (int)StepStatus.Skipped : (int)StepStatus.Pending;
                }
            }
            else
            {
                // Recovery listing after domain reload: update packages that were InProgress or Pending
                for (int i = 0; i < Packages.Length; i++)
                {
                    var status = (StepStatus)_depsStatusInt[i];
                    if (status != StepStatus.InProgress && status != StepStatus.Pending) continue;

                    bool isInstalled = Packages[i].IsNuGet
                        ? IsNewtonsoftInstalled()
                        : installed.Contains(Packages[i].PackageId);

                    if (isInstalled)
                        _depsStatusInt[i] = (int)StepStatus.Success;
                    else if (status == StepStatus.InProgress)
                        _depsStatusInt[i] = (int)StepStatus.Pending; // reset for retry
                }
            }

            _listRequest = null;
            DepsPhaseState = DepsPhase.Installing;
            _currentPkg = -1;
            AdvanceToNextPackage();
        }

        private void UpdateInstalling()
        {
            if (_addRequest == null || !_addRequest.IsCompleted) return;

            if (_addRequest.Status == StatusCode.Success)
            {
                _depsStatusInt[_currentPkg] = (int)StepStatus.Success;
            }
            else
            {
                _depsStatusInt[_currentPkg] = (int)StepStatus.Error;
                _depsErrors[_currentPkg] = _addRequest.Error?.message ?? "Unknown error";
            }

            _addRequest = null;
            AdvanceToNextPackage();
        }

        private void UpdateInstallNewtonsoft()
        {
            if (IsNewtonsoftInstalled())
            {
                int idx = Array.FindIndex(Packages, p => p.IsNuGet);
                if (idx >= 0) _depsStatusInt[idx] = (int)StepStatus.Success;
                EditorPrefs.DeleteKey(RaccoonsSetupLauncher.PendingNewtonsoftKey);
                FinishInstallation();
                return;
            }

            if (TryInstallNewtonsoftViaNuGet())
            {
                int idx = Array.FindIndex(Packages, p => p.IsNuGet);
                if (idx >= 0) _depsStatusInt[idx] = (int)StepStatus.Success;
                EditorPrefs.DeleteKey(RaccoonsSetupLauncher.PendingNewtonsoftKey);
                FinishInstallation();
            }
            // else: keep polling - waiting for NuGet PM to initialize after its domain reload
        }

        private void AdvanceToNextPackage()
        {
            _currentPkg++;

            // Skip already settled packages
            while (_currentPkg < Packages.Length && IsSettled(_depsStatusInt[_currentPkg]))
                _currentPkg++;

            if (_currentPkg >= Packages.Length)
            {
                FinishInstallation();
                return;
            }

            float total = Packages.Length;
            _progress = _currentPkg / total;
            _depsStatusInt[_currentPkg] = (int)StepStatus.InProgress;
            _progressMessage = $"Installing {Packages[_currentPkg].DisplayName}\u2026";

            if (Packages[_currentPkg].IsNuGet)
            {
                // Write packages.config so NuGet PM processes it on next reload
                EnsureNewtonsoftInPackagesConfig();
                EditorPrefs.SetBool(RaccoonsSetupLauncher.PendingNewtonsoftKey, true);
                DepsPhaseState = DepsPhase.InstallNewtonsoft;
            }
            else
            {
                DepsPhaseState = DepsPhase.Installing;
                _addRequest = Client.Add(Packages[_currentPkg].GitUrl);
            }
        }

        private static bool IsSettled(int statusInt)
        {
            var s = (StepStatus)statusInt;
            return s == StepStatus.Success || s == StepStatus.Skipped || s == StepStatus.Error;
        }

        private void FinishInstallation()
        {
            DepsPhaseState = DepsPhase.Done;
            _progress = 1f;
            _progressMessage = "Done!";
            EditorApplication.update -= OnUpdate;
            SetDepsAvailableDefine();
        }

        private static void SetDepsAvailableDefine()
        {
            const string define = "RACCOONS_DEPS_AVAILABLE";
#pragma warning disable CS0618
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            var current = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            var list = new HashSet<string>(current.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
            if (list.Add(define))
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", list));
#pragma warning restore CS0618
        }

        // ── NuGet helpers ─────────────────────────────────────────────────────

        private static bool TryInstallNewtonsoftViaNuGet()
        {
            if (IsNewtonsoftInstalled()) return true;

            Type helperType = null;
            Type identifierType = null;

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var t in asm.GetTypes())
                    {
                        if (t.Name == "NugetHelper" && helperType == null) helperType = t;
                        if (t.Name == "NugetPackageIdentifier" && identifierType == null) identifierType = t;
                    }
                }
                catch { }

                if (helperType != null && identifierType != null) break;
            }

            if (helperType == null || identifierType == null) return false;

            try
            {
                object identifier = Activator.CreateInstance(identifierType, "Newtonsoft.Json", "13.0.3");

                var method = helperType.GetMethod("InstallPackage",
                    BindingFlags.Public | BindingFlags.Static,
                    null, new[] { identifierType, typeof(bool) }, null);

                if (method != null)
                {
                    method.Invoke(null, new[] { identifier, (object)false });
                    return true;
                }

                method = helperType.GetMethod("InstallPackage",
                    BindingFlags.Public | BindingFlags.Static,
                    null, new[] { identifierType }, null);

                if (method != null)
                {
                    method.Invoke(null, new[] { identifier });
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Raccoons Setup] NuGet install attempt failed: {ex.Message}");
            }

            return false;
        }

        private static void EnsureNewtonsoftInPackagesConfig()
        {
            string projectPath = Path.GetDirectoryName(Application.dataPath);
            string configPath = Path.Combine(projectPath, "packages.config");

            XDocument doc;
            if (File.Exists(configPath))
            {
                try { doc = XDocument.Load(configPath); }
                catch { doc = new XDocument(new XElement("packages")); }
            }
            else
            {
                doc = new XDocument(new XElement("packages"));
            }

            var root = doc.Root;
            bool exists = root.Elements("package")
                .Any(e => string.Equals(e.Attribute("id")?.Value, "Newtonsoft.Json", StringComparison.OrdinalIgnoreCase));

            if (!exists)
            {
                root.Add(new XElement("package",
                    new XAttribute("id", "Newtonsoft.Json"),
                    new XAttribute("version", "13.0.3"),
                    new XAttribute("manuallyInstalled", "true")));
                doc.Save(configPath);
            }
        }

        private static bool IsNewtonsoftInstalled()
        {
            // Check NuGetForUnity packages folder
            if (Directory.Exists("Assets/Packages") &&
                Directory.GetDirectories("Assets/Packages", "Newtonsoft.Json*").Length > 0)
                return true;

            // Check packages.config (written but not yet installed — don't treat as installed)
            // Check if Newtonsoft assembly is loaded (via NuGet or UPM)
            return AppDomain.CurrentDomain.GetAssemblies()
                .Any(a => a.GetName().Name.IndexOf("Newtonsoft", StringComparison.OrdinalIgnoreCase) >= 0);
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
