using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Raccoons.Builds;

public class BuildUtilityWindow : EditorWindow
{
    private IPlatformBuildTab[] _tabs;
    private string[] _tabNames;
    private int _selectedTab;

    private SerializedObject _appConfigurationSO;

    [MenuItem("Raccoons/Build Utility")]
    public static void Open()
    {
        var window = GetWindow<BuildUtilityWindow>("Raccoons Build Utility");
        window.Show();
    }

    private void OnEnable()
    {
        _tabs = new IPlatformBuildTab[]
        {
            new AndroidBuildTab(),
            new IosBuildTab(),
            new WebBuildTab(),
            new DesktopBuildTab()
        };

        _tabNames = _tabs.Select(t => t.Name).ToArray();

        foreach (var tab in _tabs)
        {
            tab.Load();
        }

        InitAppConfigurationSerializedObject();
    }

    private void OnDisable()
    {
        if (_tabs == null) return;

        foreach (var tab in _tabs)
        {
            tab.Save();
        }
    }

    private void OnGUI()
    {
        if (_tabs == null || _tabs.Length == 0)
        {
            EditorGUILayout.LabelField("No tabs configured.");
            return;
        }

        EditorGUILayout.Space();

        DrawAppConfigurationSection();

        EditorGUILayout.Space();

        var newSelectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);
        if (newSelectedTab != _selectedTab)
        {
            _selectedTab = newSelectedTab;
            _tabs[_selectedTab].Load(); // resync from PlayerSettings on tab change
        }

        EditorGUILayout.Space();

        using (new EditorGUILayout.VerticalScope("box"))
        {
            _tabs[_selectedTab].OnGUI();
        }
    }

    private void InitAppConfigurationSerializedObject()
    {
        var config = AppConfiguration.Get();
        if (config != null)
        {
            _appConfigurationSO = new SerializedObject(config);
        }
        else
        {
            _appConfigurationSO = null;
        }
    }

    private void DrawAppConfigurationSection()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("App Configuration", EditorStyles.boldLabel);

        var config = AppConfiguration.Get();
        if (config == null)
        {
            EditorGUILayout.HelpBox("AppConfiguration asset not found at Resources/AppConfiguration.asset", MessageType.Warning);

#if UNITY_EDITOR
            if (GUILayout.Button("Create AppConfiguration asset"))
            {
                var asset = ScriptableObject.CreateInstance<AppConfiguration>();
                const string assetPath = "Assets/Resources/AppConfiguration.asset";
                var dir = Path.GetDirectoryName(assetPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                AssetDatabase.CreateAsset(asset, assetPath);
                AssetDatabase.SaveAssets();
                Selection.activeObject = asset;
                InitAppConfigurationSerializedObject();
            }
#endif
            EditorGUILayout.EndVertical();
            return;
        }

        if (_appConfigurationSO == null || _appConfigurationSO.targetObject == null)
        {
            _appConfigurationSO = new SerializedObject(config);
        }

        _appConfigurationSO.Update();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_appConfigurationSO.FindProperty("editorAppMode"));
        EditorGUILayout.PropertyField(_appConfigurationSO.FindProperty("developmentBuildAppMode"));
        EditorGUILayout.PropertyField(_appConfigurationSO.FindProperty("standardBuildAppMode"));
        EditorGUILayout.PropertyField(_appConfigurationSO.FindProperty("activateDebugObjectsInProd"));
        if (EditorGUI.EndChangeCheck())
        {
            _appConfigurationSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
        }

        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();
    }

    internal static bool ConfirmProdBuildIfNeeded()
    {
        var config = AppConfiguration.Get();
        if (config == null)
        {
            return true;
        }

        if (config.StandardBuildAppMode != AppMode.Dev)
        {
            if (config.ActivateDebugObjectsInProd)
            {
                return EditorUtility.DisplayDialog(
                    "Confirm Prod Build",
                    "Are you sure you want to activate debug objects in prod?",
                    "Yes",
                    "Cancel");
            }
            return true;
        }

        return EditorUtility.DisplayDialog(
            "Confirm Prod Build",
            "Are you sure you want to make prod build with Dev configuration?",
            "Yes",
            "Cancel");
    }

    internal static string[] GetEnabledScenes()
    {
        return EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();
    }

    internal static string GetProjectName()
    {
        return PlayerSettings.productName;
    }

    internal static string GetProjectVersion()
    {
        return PlayerSettings.bundleVersion;
    }

    internal static string GetBuildFileName(string configuration, int? buildCode, string extension = null)
    {
        var configTag = $"[{configuration}]";

        var rawName = GetProjectName();
        // Strip spaces to get names like "Cooking", "Masters" etc.
        var projectName = string.IsNullOrEmpty(rawName)
            ? "Project"
            : rawName.Replace(" ", string.Empty);

        var version = GetProjectVersion();
        var safeVersion = string.IsNullOrEmpty(version)
            ? "0_0_1"
            : version.Replace('.', '_');

        var fileName = $"{configTag}{projectName}_{safeVersion}";

        if (buildCode.HasValue && buildCode.Value > 0)
        {
            fileName += $"({buildCode.Value})";
        }

        if (!string.IsNullOrEmpty(extension))
        {
            fileName += $".{extension}";
        }

        return fileName;
    }

    internal static void RevealBuildPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return;

        EditorUtility.RevealInFinder(path);
    }
}

public interface IPlatformBuildTab
{
    string Name { get; }

    void Load();
    void Save();
    void OnGUI();
}

public abstract class PlatformBuildTabBase : IPlatformBuildTab
{
    public abstract string Name { get; }

    public abstract void Load();
    public abstract void Save();
    public abstract void OnGUI();

    protected static string IncrementPatch(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            return "0.0.1";
        }

        var parts = version.Split('.');
        if (parts.Length < 3)
        {
            // append ".1" when there is no third segment
            return version + ".1";
        }

        if (int.TryParse(parts[2], out var patch))
        {
            patch++;
            parts[2] = patch.ToString();
            return string.Join(".", parts);
        }

        return version;
    }
}

public class AndroidBuildTab : PlatformBuildTabBase
{
    private const string AndroidKeystorePassKey = "BuildUtility.Android.KeystorePass";
    private const string AndroidKeyAliasPassKey = "BuildUtility.Android.KeyAliasPass";

    private string _version;
    private int _bundleVersion;
    private string _keystorePassword;
    private string _keyAliasPassword;

    public override string Name => "Android";

    public override void Load()
    {
        _version = PlayerSettings.bundleVersion;
        _bundleVersion = PlayerSettings.Android.bundleVersionCode;
        _keystorePassword = EditorPrefs.GetString(AndroidKeystorePassKey, string.Empty);
        _keyAliasPassword = EditorPrefs.GetString(AndroidKeyAliasPassKey, string.Empty);
    }

    public override void Save()
    {
        PlayerSettings.bundleVersion = _version;
        PlayerSettings.Android.bundleVersionCode = _bundleVersion;

        EditorPrefs.SetString(AndroidKeystorePassKey, _keystorePassword ?? string.Empty);
        EditorPrefs.SetString(AndroidKeyAliasPassKey, _keyAliasPassword ?? string.Empty);
    }

    public override void OnGUI()
    {
        EditorGUILayout.LabelField("Android", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        using (new EditorGUILayout.HorizontalScope())
        {
            var newVersion = EditorGUILayout.TextField("Version", _version);
            if (newVersion != _version)
            {
                _version = newVersion;
                PlayerSettings.bundleVersion = _version;
            }

            if (GUILayout.Button("Increment patch", GUILayout.Width(130)))
            {
                _version = IncrementPatch(_version);
                PlayerSettings.bundleVersion = _version;
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            var newBundleVersion = EditorGUILayout.IntField("Bundle Version", _bundleVersion);
            if (newBundleVersion != _bundleVersion)
            {
                _bundleVersion = newBundleVersion;
                PlayerSettings.Android.bundleVersionCode = _bundleVersion;
            }

            if (GUILayout.Button("Increment", GUILayout.Width(130)))
            {
                _bundleVersion++;
                PlayerSettings.Android.bundleVersionCode = _bundleVersion;
            }
        }

        EditorGUILayout.Space();

        // Keystore / alias passwords (stored only in EditorPrefs, applied on build)
        using (new EditorGUILayout.HorizontalScope())
        {
            var newKeystorePass = EditorGUILayout.PasswordField("Keystore Password", _keystorePassword);
            if (newKeystorePass != _keystorePassword)
            {
                _keystorePassword = newKeystorePass;
                EditorPrefs.SetString(AndroidKeystorePassKey, _keystorePassword ?? string.Empty);
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            var newAliasPass = EditorGUILayout.PasswordField("Key Alias Password", _keyAliasPassword);
            if (newAliasPass != _keyAliasPassword)
            {
                _keyAliasPassword = newAliasPass;
                EditorPrefs.SetString(AndroidKeyAliasPassKey, _keyAliasPassword ?? string.Empty);
            }
        }

        EditorGUILayout.Space();

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Build Dev AAB"))
            {
                BuildAndroid(true, true);
            }

            if (GUILayout.Button("Build Dev APK"))
            {
                BuildAndroid(true, false);
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Build Prod AAB"))
            {
                BuildAndroid(false, true);
            }

            if (GUILayout.Button("Build Prod APK"))
            {
                BuildAndroid(false, false);
            }
        }
    }

    private void BuildAndroid(bool development, bool appBundle)
    {
        Save();

        if (!development)
        {
            if (!BuildUtilityWindow.ConfirmProdBuildIfNeeded())
                return;
        }

        if (!string.IsNullOrEmpty(_keystorePassword))
            PlayerSettings.Android.keystorePass = _keystorePassword;

        if (!string.IsNullOrEmpty(_keyAliasPassword))
            PlayerSettings.Android.keyaliasPass = _keyAliasPassword;

        var config = development ? "Dev" : "Prod";
        var extension = appBundle ? "aab" : "apk";

        var root = "Builds/Android";
        var dir = Path.Combine(root, config);
        Directory.CreateDirectory(dir);

        var fileName = BuildUtilityWindow.GetBuildFileName(config, _bundleVersion, extension);
        var outputPath = Path.Combine(dir, fileName);

        var options = new BuildPlayerOptions
        {
            scenes = BuildUtilityWindow.GetEnabledScenes(),
            target = BuildTarget.Android,
            options = development ? BuildOptions.Development : BuildOptions.None,
            locationPathName = outputPath
        };

        EditorUserBuildSettings.buildAppBundle = appBundle;

        var report = BuildPipeline.BuildPlayer(options);
        LogReport(report, outputPath);
    }

    private static void LogReport(BuildReport report, string outputPath)
    {
        if (report == null) return;

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"Android build succeeded: {report.summary.totalSize / (1024 * 1024f):0.0} MB");
            BuildUtilityWindow.RevealBuildPath(outputPath);
        }
        else
        {
            Debug.LogError("Android build failed: " + report.summary.result);
        }
    }
}

public class IosBuildTab : PlatformBuildTabBase
{
    private string _version;
    private int _build;

    public override string Name => "iOS";

    public override void Load()
    {
        _version = PlayerSettings.bundleVersion;
        _build = SafeParseBuildNumber(PlayerSettings.iOS.buildNumber);
    }

    private static int SafeParseBuildNumber(string buildNumber)
    {
        return int.TryParse(buildNumber, out var result) ? result : 1;
    }

    public override void Save()
    {
        PlayerSettings.bundleVersion = _version;
        PlayerSettings.iOS.buildNumber = _build.ToString();
    }

    public override void OnGUI()
    {
        EditorGUILayout.LabelField("iOS", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        using (new EditorGUILayout.HorizontalScope())
        {
            var newVersion = EditorGUILayout.TextField("Version", _version);
            if (newVersion != _version)
            {
                _version = newVersion;
                PlayerSettings.bundleVersion = _version;
            }

            if (GUILayout.Button("Increment patch", GUILayout.Width(130)))
            {
                _version = IncrementPatch(_version);
                PlayerSettings.bundleVersion = _version;
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            var newBuild = EditorGUILayout.IntField("Build", _build);
            if (newBuild != _build)
            {
                _build = newBuild;
                PlayerSettings.iOS.buildNumber = _build.ToString();
            }

            if (GUILayout.Button("Increment", GUILayout.Width(130)))
            {
                _build++;
                PlayerSettings.iOS.buildNumber = _build.ToString();
            }
        }

        EditorGUILayout.Space();

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Build Dev"))
            {
                BuildIos(true);
            }

            if (GUILayout.Button("Build Prod"))
            {
                BuildIos(false);
            }
        }
    }

    private void BuildIos(bool development)
    {
        Save();

        if (!development)
        {
            if (!BuildUtilityWindow.ConfirmProdBuildIfNeeded())
                return;
        }

        var config = development ? "Dev" : "Prod";
        var root = "Builds/iOS";
        var dir = Path.Combine(root, config);
        Directory.CreateDirectory(dir);

        var fileName = BuildUtilityWindow.GetBuildFileName(config, _build, null);
        var outputPath = Path.Combine(dir, fileName);

        var options = new BuildPlayerOptions
        {
            scenes = BuildUtilityWindow.GetEnabledScenes(),
            target = BuildTarget.iOS,
            options = development ? BuildOptions.Development : BuildOptions.None,
            locationPathName = outputPath
        };

        var report = BuildPipeline.BuildPlayer(options);
        LogReport(report, outputPath);
    }

    private static void LogReport(BuildReport report, string outputPath)
    {
        if (report == null) return;

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("iOS build succeeded.");
            BuildUtilityWindow.RevealBuildPath(outputPath);
        }
        else
        {
            Debug.LogError("iOS build failed: " + report.summary.result);
        }
    }
}

public class WebBuildTab : PlatformBuildTabBase
{
    private string _version;

    public override string Name => "Web";

    public override void Load()
    {
        _version = PlayerSettings.bundleVersion;
    }

    public override void Save()
    {
        PlayerSettings.bundleVersion = _version;
    }

    public override void OnGUI()
    {
        EditorGUILayout.LabelField("WebGL", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        using (new EditorGUILayout.HorizontalScope())
        {
            var newVersion = EditorGUILayout.TextField("Version", _version);
            if (newVersion != _version)
            {
                _version = newVersion;
                PlayerSettings.bundleVersion = _version;
            }

            if (GUILayout.Button("Increment patch", GUILayout.Width(130)))
            {
                _version = IncrementPatch(_version);
                PlayerSettings.bundleVersion = _version;
            }
        }

        EditorGUILayout.Space();

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Build Dev"))
            {
                BuildWeb(true);
            }

            if (GUILayout.Button("Build Prod"))
            {
                BuildWeb(false);
            }
        }
    }

    private void BuildWeb(bool development)
    {
        Save();

        if (!development)
        {
            if (!BuildUtilityWindow.ConfirmProdBuildIfNeeded())
                return;
        }

        var config = development ? "Dev" : "Prod";
        var root = "Builds/WebGL";
        var dir = Path.Combine(root, config);
        Directory.CreateDirectory(dir);

        // Web has no explicit build code -> omit parentheses
        var fileName = BuildUtilityWindow.GetBuildFileName(config, null, null);
        var outputPath = Path.Combine(dir, fileName);

        var options = new BuildPlayerOptions
        {
            scenes = BuildUtilityWindow.GetEnabledScenes(),
            target = BuildTarget.WebGL,
            options = development ? BuildOptions.Development : BuildOptions.None,
            locationPathName = outputPath
        };

        var report = BuildPipeline.BuildPlayer(options);
        LogReport(report, outputPath);
    }

    private static void LogReport(BuildReport report, string outputPath)
    {
        if (report == null) return;

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("WebGL build succeeded.");
            BuildUtilityWindow.RevealBuildPath(outputPath);
        }
        else
        {
            Debug.LogError("WebGL build failed: " + report.summary.result);
        }
    }
}

public class DesktopBuildTab : PlatformBuildTabBase
{
    private const string DesktopBuildKey = "BuildUtility.Desktop.Build";

    private string _version;
    private int _build;

    public override string Name => "Desktop";

    public override void Load()
    {
        _version = PlayerSettings.bundleVersion;
        _build = EditorPrefs.GetInt(DesktopBuildKey, 1);
    }

    public override void Save()
    {
        PlayerSettings.bundleVersion = _version;
        EditorPrefs.SetInt(DesktopBuildKey, _build);
    }

    public override void OnGUI()
    {
        EditorGUILayout.LabelField("Desktop", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        using (new EditorGUILayout.HorizontalScope())
        {
            var newVersion = EditorGUILayout.TextField("Version", _version);
            if (newVersion != _version)
            {
                _version = newVersion;
                PlayerSettings.bundleVersion = _version;
            }

            if (GUILayout.Button("Increment patch", GUILayout.Width(130)))
            {
                _version = IncrementPatch(_version);
                PlayerSettings.bundleVersion = _version;
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            var newBuild = EditorGUILayout.IntField("Build", _build);
            if (newBuild != _build)
            {
                _build = newBuild;
                EditorPrefs.SetInt(DesktopBuildKey, _build);
            }

            if (GUILayout.Button("Increment", GUILayout.Width(130)))
            {
                _build++;
                EditorPrefs.SetInt(DesktopBuildKey, _build);
            }
        }

        EditorGUILayout.Space();

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Build Dev"))
            {
                BuildDesktop(true);
            }

            if (GUILayout.Button("Build Prod"))
            {
                BuildDesktop(false);
            }
        }
    }

    private void BuildDesktop(bool development)
    {
        Save();

        if (!development)
        {
            if (!BuildUtilityWindow.ConfirmProdBuildIfNeeded())
                return;
        }

        // Build for current active standalone target
        var target = EditorUserBuildSettings.activeBuildTarget;
        if (target != BuildTarget.StandaloneWindows &&
            target != BuildTarget.StandaloneWindows64 &&
            target != BuildTarget.StandaloneOSX &&
            target != BuildTarget.StandaloneLinux64)
        {
            Debug.LogError("Active build target is not a standalone platform.");
            return;
        }

        var config = development ? "Dev" : "Prod";
        var root = "Builds/Desktop";
        var dir = Path.Combine(root, config, target.ToString());
        Directory.CreateDirectory(dir);

        string extension = null;
        switch (target)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                extension = "exe";
                break;
            case BuildTarget.StandaloneOSX:
                extension = "app";
                break;
        }

        var fileName = BuildUtilityWindow.GetBuildFileName(config, _build, extension);
        var outputPath = Path.Combine(dir, fileName);

        var options = new BuildPlayerOptions
        {
            scenes = BuildUtilityWindow.GetEnabledScenes(),
            target = target,
            options = development ? BuildOptions.Development : BuildOptions.None,
            locationPathName = outputPath
        };

        var report = BuildPipeline.BuildPlayer(options);
        LogReport(report, outputPath);
    }

    private static void LogReport(BuildReport report, string outputPath)
    {
        if (report == null) return;

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Desktop build succeeded.");
            BuildUtilityWindow.RevealBuildPath(outputPath);
        }
        else
        {
            Debug.LogError("Desktop build failed: " + report.summary.result);
        }
    }
}
