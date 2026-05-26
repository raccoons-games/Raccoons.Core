using System.Collections.Generic;
using System.Text;
using Raccoons.Analytics;
using Raccoons.Analytics.Firebase;
using UnityEngine;

namespace Raccoons.Core.Samples.Analytics
{
    public class AnalyticsLevelFlowSample : MonoBehaviour
    {
        private int _missionId = 1;
        private readonly string _missionType = "level";
        private readonly Dictionary<int, int> _attemptTracker = new();
        private readonly List<string> _eventLog = new();
        private Vector2 _logScrollPos;

        private GUIStyle _headerStyle;
        private GUIStyle _sectionLabelStyle;
        private GUIStyle _groupLabelStyle;
        private GUIStyle _logEntryStyle;
        private GUIStyle _logBoxStyle;
        private bool _stylesInit;

        private static readonly string[] KeyParamsToShow =
        {
            "mission_id", "mission_attempt", "fail_reason",
            "power_up_name", "step_name", "placement", "revived",
            "reward_type", "reward_name", "reward_amount"
        };

        private void Awake()
        {
            RaccoonsAnalytics.OnLogEvent += OnAnalyticsEvent;
            RaccoonsAnalytics.Initialize(new FirebaseAnalyticsService());
        }

        private void OnDestroy()
        {
            RaccoonsAnalytics.OnLogEvent -= OnAnalyticsEvent;
        }

        private void OnAnalyticsEvent(AnalyticsEvent e)
        {
            var sb = new StringBuilder($"● {e.EventName}");
            var parts = new List<string>();
            foreach (var key in KeyParamsToShow)
                if (e.Parameters.TryGetValue(key, out var val))
                    parts.Add($"{key.Replace("mission_", "")}={val}");
            if (parts.Count > 0) sb.Append($"  ({string.Join(", ", parts)})");
            _eventLog.Insert(0, sb.ToString());
            if (_eventLog.Count > 30) _eventLog.RemoveAt(_eventLog.Count - 1);
        }

        private void OnGUI()
        {
            EnsureStyles();

            // Scale UI to a reference width of 480 logical pixels so it looks
            // consistent on both desktop and high-DPI mobile screens.
            float scale = Screen.width / 480f;
            Matrix4x4 prevMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));

            int logicalHeight = Mathf.RoundToInt(Screen.height / scale);

            var panelRect = new Rect(10, 10, 460, logicalHeight - 20);
            GUI.Box(panelRect, GUIContent.none);
            GUILayout.BeginArea(new Rect(20, 20, 440, logicalHeight - 40));

            GUILayout.Label("Analytics Level Flow Demo", _headerStyle);
            GUILayout.Space(6);

            DrawMissionConfig();
            GUILayout.Space(10);
            DrawFlowButtons();
            GUILayout.Space(10);
            DrawEventLog();

            GUILayout.EndArea();
            GUI.matrix = prevMatrix;
        }

        private void DrawMissionConfig()
        {
            GUILayout.Label("Mission Config", _sectionLabelStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Mission ID:", GUILayout.Width(80));
            if (GUILayout.Button("◀", GUILayout.Width(26)) && _missionId > 1)
            {
                _missionId--;
            }
            GUILayout.Label(_missionId.ToString("D2"), GUILayout.Width(28));
            if (GUILayout.Button("▶", GUILayout.Width(26)))
            {
                _missionId++;
            }
            GUILayout.Space(16);
            GUILayout.Label($"Type: {_missionType}", GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            if (_attemptTracker.TryGetValue(_missionId, out int currentAttempt))
                GUILayout.Label($"Next attempt for this ID: {currentAttempt + 1}", EditorHintStyle());
            else
                GUILayout.Label("First attempt", EditorHintStyle());
        }

        private void DrawFlowButtons()
        {
            GUILayout.Label("Process Level Flow", _sectionLabelStyle);

            DrawFlowGroup("Basic",
                ("Start → Complete",                       (System.Action)RunFlow3),
                ("Start → PowerUp → Complete",             RunFlow1),
                ("Start → Checkpoint × 2 → Complete",     RunFlow6)
            );

            GUILayout.Space(4);

            DrawFlowGroup("Fail & Recovery",
                ("Start → Soft Fail → Revive → Complete",  RunFlow2),
                ("Start → Soft Fail → Ad Revive → Complete", RunFlow7),
                ("Start → Hard Fail (no revive)",          RunFlow5),
                ("Start → Abandoned",                      RunFlow4)
            );

            GUILayout.Space(4);

            DrawFlowGroup("Complex",
                ("Start → Step → PowerUp → Item → Step → Complete", RunFlow8)
            );
        }

        private void DrawFlowGroup(string groupName, params (string label, System.Action action)[] flows)
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label(groupName, _groupLabelStyle);
            GUILayout.Space(2);
            foreach (var (label, action) in flows)
            {
                if (GUILayout.Button(label, GUILayout.Height(28)))
                    action();
                GUILayout.Space(2);
            }
            GUILayout.EndVertical();
        }

        private void DrawEventLog()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Event Log", _sectionLabelStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Clear", GUILayout.Width(50)))
                _eventLog.Clear();
            GUILayout.EndHorizontal();

            _logScrollPos = GUILayout.BeginScrollView(_logScrollPos, _logBoxStyle, GUILayout.ExpandHeight(true));
            if (_eventLog.Count == 0)
            {
                GUILayout.Label("No events yet. Press a Flow button.", EditorHintStyle());
            }
            else
            {
                foreach (var entry in _eventLog)
                    GUILayout.Label(entry, _logEntryStyle);
            }
            GUILayout.EndScrollView();
        }

        // ── Level flows ───────────────────────────────────────────────────────

        private void RunFlow1()
        {
            int attempt = NextAttempt(_missionId);
            string name = MissionName(_missionId);

            RaccoonsAnalytics.MissionStarted(_missionType, name, _missionId, attempt);

            RaccoonsAnalytics.PowerUpUsed(
                missionID: _missionId.ToString(),
                missionType: _missionType,
                missionAttempt: attempt,
                powerUpName: "shield",
                missionName: name
            );

            RaccoonsAnalytics.MissionCompleted(
                _missionType, name, _missionId,
                missionAttempt: attempt,
                reward: BuildCoinsReward(50 + _missionId * 10)
            );
        }

        private void RunFlow2()
        {
            int attempt1 = NextAttempt(_missionId);
            string name = MissionName(_missionId);

            RaccoonsAnalytics.MissionStarted(_missionType, name, _missionId, attempt1);

            RaccoonsAnalytics.MissionFailed(
                _missionType, name, _missionId,
                missionAttempt: attempt1,
                failReason: "soft_fail"
            );

            int attempt2 = NextAttempt(_missionId);

            RaccoonsAnalytics.MissionStarted(
                _missionType, name, _missionId,
                missionAttempt: attempt2,
                additionalData: new Dictionary<string, object> { { "revived", true } }
            );

            RaccoonsAnalytics.MissionCompleted(
                _missionType, name, _missionId,
                missionAttempt: attempt2
            );
        }

        private void RunFlow3()
        {
            int attempt = NextAttempt(_missionId);
            string name = MissionName(_missionId);

            RaccoonsAnalytics.MissionStarted(_missionType, name, _missionId, attempt);

            RaccoonsAnalytics.MissionCompleted(
                _missionType, name, _missionId,
                missionAttempt: attempt
            );
        }

        // Flow 4: Start → Abandoned
        private void RunFlow4()
        {
            int attempt = NextAttempt(_missionId);
            string name = MissionName(_missionId);

            RaccoonsAnalytics.MissionStarted(_missionType, name, _missionId, attempt);
            RaccoonsAnalytics.MissionAbandoned(_missionType, name, _missionId, attempt);
        }

        // Flow 5: Start → Hard Fail (no revive)
        private void RunFlow5()
        {
            int attempt = NextAttempt(_missionId);
            string name = MissionName(_missionId);

            RaccoonsAnalytics.MissionStarted(_missionType, name, _missionId, attempt);

            RaccoonsAnalytics.MissionFailed(
                _missionType, name, _missionId,
                missionAttempt: attempt,
                failReason: "time_out"
            );
        }

        // Flow 6: Start → Checkpoint → Checkpoint → Complete
        private void RunFlow6()
        {
            int attempt = NextAttempt(_missionId);
            string name = MissionName(_missionId);

            RaccoonsAnalytics.MissionStarted(_missionType, name, _missionId, attempt);

            RaccoonsAnalytics.MissionStep(
                _missionType, name, _missionId,
                missionAttempt: attempt,
                reward: BuildCoinsReward(25),
                stepName: "checkpoint_1"
            );

            RaccoonsAnalytics.MissionStep(
                _missionType, name, _missionId,
                missionAttempt: attempt,
                reward: BuildCoinsReward(25),
                stepName: "checkpoint_2"
            );

            RaccoonsAnalytics.MissionCompleted(
                _missionType, name, _missionId,
                missionAttempt: attempt,
                reward: BuildCoinsReward(100 + _missionId * 10)
            );
        }

        // Flow 7: Start → Soft Fail → Rewarded Video Revive → Start(attempt2) → Complete
        private void RunFlow7()
        {
            int attempt1 = NextAttempt(_missionId);
            string name = MissionName(_missionId);
            const string placement = "RevivePrompt";
            const string network = "applovin";

            RaccoonsAnalytics.MissionStarted(_missionType, name, _missionId, attempt1);

            RaccoonsAnalytics.MissionFailed(
                _missionType, name, _missionId,
                missionAttempt: attempt1,
                failReason: "soft_fail"
            );

            RaccoonsAnalytics.RewardVideoTryShow(placement);
            RaccoonsAnalytics.RewardVideoShow(placement, network);
            RaccoonsAnalytics.RewardVideoCollect(
                placement,
                reward: new Reward(new Product
                {
                    VirtualCurrencies = new List<VirtualCurrency> { new VirtualCurrency("revive", "utility", 1) }
                })
            );

            int attempt2 = NextAttempt(_missionId);

            RaccoonsAnalytics.MissionStarted(
                _missionType, name, _missionId,
                missionAttempt: attempt2,
                additionalData: new Dictionary<string, object> { { "revived", true } }
            );

            RaccoonsAnalytics.MissionCompleted(
                _missionType, name, _missionId,
                missionAttempt: attempt2,
                reward: BuildCoinsReward(50 + _missionId * 10)
            );
        }

        // Flow 8: Full complex — Step + PowerUp + ItemCollected + Step + Complete
        private void RunFlow8()
        {
            int attempt = NextAttempt(_missionId);
            string name = MissionName(_missionId);

            RaccoonsAnalytics.MissionStarted(_missionType, name, _missionId, attempt);

            RaccoonsAnalytics.MissionStep(
                _missionType, name, _missionId,
                missionAttempt: attempt,
                stepName: "boss_appeared"
            );

            RaccoonsAnalytics.PowerUpUsed(
                missionID: _missionId.ToString(),
                missionType: _missionType,
                missionAttempt: attempt,
                powerUpName: "double_damage",
                missionName: name
            );

            RaccoonsAnalytics.ItemCollected(
                reward: BuildCoinsReward(50),
                additionalData: new Dictionary<string, object> { { "source", "chest" } }
            );

            RaccoonsAnalytics.MissionStep(
                _missionType, name, _missionId,
                missionAttempt: attempt,
                stepName: "boss_defeated"
            );

            RaccoonsAnalytics.MissionCompleted(
                _missionType, name, _missionId,
                missionAttempt: attempt,
                reward: BuildCoinsReward(200 + _missionId * 15)
            );
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private int NextAttempt(int missionId)
        {
            _attemptTracker.TryGetValue(missionId, out int current);
            _attemptTracker[missionId] = current + 1;
            return current + 1;
        }

        private static string MissionName(int id) => $"level_{id}";

        private static Reward BuildCoinsReward(int amount) =>
            new Reward(new Product
            {
                VirtualCurrencies = new List<VirtualCurrency> { new VirtualCurrency("coins", "soft", amount) }
            });

        // ── Styles ────────────────────────────────────────────────────────────

        private void EnsureStyles()
        {
            if (_stylesInit && _headerStyle != null) return;

            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold
            };
            _sectionLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.7f, 0.85f, 1f) }
            };
            _groupLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }
            };
            _logEntryStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                wordWrap = false,
                normal = { textColor = new Color(0.75f, 0.95f, 0.75f) }
            };
            _logBoxStyle = new GUIStyle("box");
            _stylesInit = true;
        }

        private static GUIStyle EditorHintStyle()
        {
            return new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                normal = { textColor = new Color(0.55f, 0.55f, 0.55f) }
            };
        }
    }
}
