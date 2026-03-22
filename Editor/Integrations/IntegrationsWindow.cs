using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Raccoons.Editor
{
    public class IntegrationsWindow : EditorWindow
    {
        private static readonly IntegrationDefinition[] AllIntegrations =
        {
            new IntegrationDefinition(
                name: "SRDebugger",
                symbol: "RACCOONS_INTEGRATION_SRDEBUGGER",
                category: "Debug",
                description: "Runtime debug panel with console, profiler, options system, and bug reporter. Tap the corner trigger to open at runtime.",
                packageId: "com.stompyrobot.srdebugger",
                detectionType: "SRDebugger.Settings",
                assemblyName: "StompyRobot.SRDebugger",
                documentationUrl: "https://stompyrobot.uk/tools/srdebugger/documentation/"
            ),
        };

        private static readonly string[] TabNames = { "All", "Enabled", "Detected" };

        private Vector2 _scrollPosition;
        private string _searchFilter = string.Empty;
        private bool _applyToAllPlatforms;
        private int _selectedTab;
        private bool[] _enabledStates;

        // Styles (initialized lazily in OnGUI to avoid domain-reload issues)
        private GUIStyle _headerTitleStyle;
        private GUIStyle _headerSubtitleStyle;
        private GUIStyle _cardStyle;
        private GUIStyle _integrationNameStyle;
        private GUIStyle _descriptionStyle;
        private GUIStyle _symbolStyle;
        private GUIStyle _enabledButtonStyle;
        private GUIStyle _disabledButtonStyle;
        private GUIStyle _categoryLabelStyle;
        private GUIStyle _warningStyle;
        private bool _stylesInitialized;

        [MenuItem("Raccoons/Integrations")]
        public static void Open()
        {
            var window = GetWindow<IntegrationsWindow>("Integrations");
            window.minSize = new Vector2(480, 420);
            window.Show();
        }

        private void OnEnable()
        {
            RefreshStates();
        }

        private void RefreshStates()
        {
            _enabledStates = new bool[AllIntegrations.Length];
            var defines = GetCurrentDefines();
            for (var i = 0; i < AllIntegrations.Length; i++)
                _enabledStates[i] = defines.Contains(AllIntegrations[i].Symbol);
        }

        private HashSet<string> GetCurrentDefines()
        {
            var raw = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup);
            return new HashSet<string>(
                raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private void SetDefineEnabled(string symbol, bool enabled)
        {
            if (_applyToAllPlatforms)
            {
                foreach (BuildTargetGroup group in Enum.GetValues(typeof(BuildTargetGroup)))
                {
                    if (group == BuildTargetGroup.Unknown) continue;
                    try
                    {
                        var raw = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
                        var defines = new HashSet<string>(
                            raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                        if (enabled) defines.Add(symbol);
                        else defines.Remove(symbol);
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(
                            group, string.Join(";", defines));
                    }
                    catch { /* some groups may not be valid targets */ }
                }
            }
            else
            {
                var defines = GetCurrentDefines();
                if (enabled) defines.Add(symbol);
                else defines.Remove(symbol);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    EditorUserBuildSettings.selectedBuildTargetGroup,
                    string.Join(";", defines));
            }

            RefreshStates();
        }

        private void OnGUI()
        {
            EnsureStyles();
            DrawHeader();
            DrawToolbar();
            DrawIntegrationsList();
        }

        // ── Header ────────────────────────────────────────────────────────────

        private void DrawHeader()
        {
            var headerRect = GUILayoutUtility.GetRect(0, 64, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(headerRect, new Color(0.13f, 0.13f, 0.13f));

            GUI.Label(new Rect(headerRect.x + 16, headerRect.y + 10, headerRect.width - 32, 28),
                "Integrations", _headerTitleStyle);
            GUI.Label(new Rect(headerRect.x + 16, headerRect.y + 38, headerRect.width - 32, 18),
                "Manage third-party integrations for your Raccoons project", _headerSubtitleStyle);
        }

        // ── Toolbar ───────────────────────────────────────────────────────────

        private void DrawToolbar()
        {
            EditorGUILayout.Space(6);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(12);
                var newFilter = EditorGUILayout.TextField(
                    _searchFilter, EditorStyles.toolbarSearchField, GUILayout.ExpandWidth(true));
                if (newFilter != _searchFilter)
                {
                    _searchFilter = newFilter;
                    Repaint();
                }
                GUILayout.Space(12);
            }

            EditorGUILayout.Space(4);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(12);
                var newTab = GUILayout.Toolbar(_selectedTab, TabNames, GUILayout.ExpandWidth(true));
                if (newTab != _selectedTab)
                {
                    _selectedTab = newTab;
                    Repaint();
                }
                GUILayout.Space(12);
            }

            EditorGUILayout.Space(4);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(12);
                _applyToAllPlatforms = EditorGUILayout.ToggleLeft(
                    new GUIContent("Apply to all platforms",
                        "When enabled, changes affect define symbols on all build target groups, not just the currently selected one."),
                    _applyToAllPlatforms, GUILayout.ExpandWidth(false));
                GUILayout.FlexibleSpace();
                GUILayout.Space(12);
            }

            EditorGUILayout.Space(6);
            DrawSeparator();
        }

        // ── Integrations list ─────────────────────────────────────────────────

        private void DrawIntegrationsList()
        {
            var query = _searchFilter.Trim().ToLowerInvariant();

            IEnumerable<IntegrationDefinition> source = AllIntegrations;

            // Tab filter
            switch (_selectedTab)
            {
                case 1: // Enabled
                    source = source.Where(i => _enabledStates[Array.IndexOf(AllIntegrations, i)]);
                    break;
                case 2: // Detected
                    source = source.Where(i => i.IsPackageInstalled());
                    break;
            }

            var filtered = string.IsNullOrEmpty(query)
                ? source.ToArray()
                : source.Where(i =>
                    i.Name.ToLowerInvariant().Contains(query) ||
                    i.Category.ToLowerInvariant().Contains(query) ||
                    i.Description.ToLowerInvariant().Contains(query)).ToArray();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            if (filtered.Length == 0)
            {
                EditorGUILayout.Space(32);
                var emptyMessage = _selectedTab switch
                {
                    1 => "No integrations are currently enabled.",
                    2 => "No integrations were detected in this project.",
                    _ => "No integrations match your search."
                };
                EditorGUILayout.LabelField(emptyMessage, EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                var categories = filtered.Select(i => i.Category).Distinct().OrderBy(c => c);
                foreach (var category in categories)
                {
                    DrawCategorySection(category,
                        filtered.Where(i => i.Category == category).ToArray());
                }
            }

            EditorGUILayout.Space(12);
            EditorGUILayout.EndScrollView();
        }

        private void DrawCategorySection(string category, IntegrationDefinition[] integrations)
        {
            EditorGUILayout.Space(10);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(12);
                EditorGUILayout.LabelField(category.ToUpperInvariant(),
                    _categoryLabelStyle, GUILayout.Width(100));
                var lineRect = GUILayoutUtility.GetRect(0, 1, GUILayout.ExpandWidth(true));
                lineRect.y += lineRect.height / 2 + 4;
                EditorGUI.DrawRect(lineRect, new Color(0.3f, 0.3f, 0.3f));
                GUILayout.Space(12);
            }

            EditorGUILayout.Space(6);

            foreach (var integration in integrations)
            {
                var index = Array.IndexOf(AllIntegrations, integration);
                DrawIntegrationCard(integration, index);
                EditorGUILayout.Space(4);
            }
        }

        private void DrawIntegrationCard(IntegrationDefinition integration, int index)
        {
            var isEnabled = _enabledStates[index];
            var isInstalled = integration.IsPackageInstalled();

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(12);

                using (new EditorGUILayout.VerticalScope(_cardStyle, GUILayout.ExpandWidth(true)))
                {
                    // ── Row 1: indicator dot, name, status, button ──
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        DrawStatusDot(isEnabled, isInstalled);
                        GUILayout.Space(8);

                        EditorGUILayout.LabelField(integration.Name, _integrationNameStyle);

                        GUILayout.FlexibleSpace();

                        if (!isInstalled)
                        {
                            var prevColor = GUI.color;
                            GUI.color = new Color(1f, 0.75f, 0.2f);
                            GUILayout.Label(new GUIContent("⚠ Not detected",
                                $"Package '{integration.PackageId}' was not detected in this project."),
                                _warningStyle, GUILayout.ExpandWidth(false));
                            GUI.color = prevColor;
                            GUILayout.Space(6);
                        }

                        DrawToggleButton(integration, index, isEnabled);
                    }

                    EditorGUILayout.Space(4);

                    // ── Row 2: description ──
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Space(20);
                        EditorGUILayout.LabelField(integration.Description, _descriptionStyle);
                    }

                    EditorGUILayout.Space(6);

                    // ── Row 3: symbol + docs link ──
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Space(20);

                        EditorGUILayout.LabelField(integration.Symbol, _symbolStyle,
                            GUILayout.ExpandWidth(false));

                        GUILayout.Space(6);

                        if (GUILayout.Button(new GUIContent("⎘", "Copy symbol to clipboard"),
                                EditorStyles.miniButton, GUILayout.Width(22), GUILayout.Height(16)))
                        {
                            EditorGUIUtility.systemCopyBuffer = integration.Symbol;
                        }

                        GUILayout.FlexibleSpace();

                        if (!string.IsNullOrEmpty(integration.DocumentationUrl))
                        {
                            if (GUILayout.Button("Docs →", EditorStyles.linkLabel,
                                    GUILayout.ExpandWidth(false)))
                                Application.OpenURL(integration.DocumentationUrl);
                        }

                        GUILayout.Space(4);
                    }

                    EditorGUILayout.Space(2);
                }

                GUILayout.Space(12);
            }
        }

        private void DrawToggleButton(IntegrationDefinition integration, int index, bool isEnabled)
        {
            var label = isEnabled ? "Disable" : "Enable";
            var style = isEnabled ? _disabledButtonStyle : _enabledButtonStyle;

            if (GUILayout.Button(label, style, GUILayout.Width(76), GUILayout.Height(22)))
            {
                var confirm = !isEnabled || EditorUtility.DisplayDialog(
                    "Disable Integration",
                    $"Disable '{integration.Name}'?\n\nThis will remove the define symbol '{integration.Symbol}' and may cause compile errors if the integration is referenced in your code.",
                    "Disable", "Cancel");

                if (confirm)
                    SetDefineEnabled(integration.Symbol, !isEnabled);
            }
        }

        // ── Drawing helpers ───────────────────────────────────────────────────

        private static void DrawStatusDot(bool isEnabled, bool isInstalled)
        {
            var dotRect = GUILayoutUtility.GetRect(10, 10, GUILayout.Width(10), GUILayout.Height(10));
            dotRect.y += 5;

            Color color;
            if (!isInstalled)
                color = new Color(0.9f, 0.6f, 0.1f);
            else if (isEnabled)
                color = new Color(0.2f, 0.85f, 0.35f);
            else
                color = new Color(0.45f, 0.45f, 0.45f);

            EditorGUI.DrawRect(dotRect, color);
        }

        private static void DrawSeparator()
        {
            var rect = GUILayoutUtility.GetRect(0, 1, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(0.25f, 0.25f, 0.25f));
        }

        // ── Style initialization ──────────────────────────────────────────────

        private void EnsureStyles()
        {
            // Re-init after domain reload or skin change
            if (_stylesInitialized && _cardStyle != null) return;

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

            _cardStyle = new GUIStyle("box")
            {
                padding = new RectOffset(12, 12, 10, 10),
                margin = new RectOffset(0, 0, 0, 0)
            };

            _integrationNameStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13
            };

            _descriptionStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                wordWrap = true,
                normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
            };

            _symbolStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontStyle = FontStyle.Italic,
                normal = { textColor = new Color(0.5f, 0.75f, 1f) }
            };

            _categoryLabelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 10,
                normal = { textColor = new Color(0.55f, 0.55f, 0.55f) }
            };

            _warningStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontStyle = FontStyle.Bold
            };

            _enabledButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.25f, 0.85f, 0.4f) },
                hover = { textColor = new Color(0.3f, 1f, 0.5f) }
            };

            _disabledButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                normal = { textColor = new Color(0.85f, 0.4f, 0.4f) },
                hover = { textColor = new Color(1f, 0.5f, 0.5f) }
            };

            _stylesInitialized = true;
        }
    }

    // ── Integration data ──────────────────────────────────────────────────────

    public class IntegrationDefinition
    {
        public string Name { get; }
        public string Symbol { get; }
        public string Category { get; }
        public string Description { get; }
        public string PackageId { get; }
        public string DetectionType { get; }
        public string AssemblyName { get; }
        public string DocumentationUrl { get; }

        public IntegrationDefinition(
            string name, string symbol, string category,
            string description, string packageId = "",
            string detectionType = "", string assemblyName = "",
            string documentationUrl = "")
        {
            Name = name;
            Symbol = symbol;
            Category = category;
            Description = description;
            PackageId = packageId;
            DetectionType = detectionType;
            AssemblyName = assemblyName;
            DocumentationUrl = documentationUrl;
        }

        /// <summary>
        /// Returns true if the package is detected via type lookup (works for both UPM and
        /// .unitypackage installs) or via assembly definition asset (UPM installs).
        /// Returns true when no detection hints are specified.
        /// </summary>
        public bool IsPackageInstalled()
        {
            if (!string.IsNullOrEmpty(DetectionType))
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetType(DetectionType) != null) return true;
                }
            }

            if (!string.IsNullOrEmpty(AssemblyName))
            {
                if (AssetDatabase.FindAssets($"t:AssemblyDefinitionAsset {AssemblyName}").Length > 0)
                    return true;
            }

            return string.IsNullOrEmpty(DetectionType) && string.IsNullOrEmpty(AssemblyName);
        }
    }
}
