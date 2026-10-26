using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Raccoons.Analytics
{
    public static partial class RaccoonsAnalytics
    {
        private static readonly List<IAnalyticsService> _services = new();
        // Read without a lock: ad revenue callbacks may arrive off the main thread.
        private static volatile IAnalyticsService[] _serviceSnapshot = Array.Empty<IAnalyticsService>();
        private static readonly Queue<Action> _pendingEvents = new();
        private static readonly object _lock = new();
        private static volatile bool _holdEvents;

        private static int? _playerLevel;
        private static int? _playerXp;
        private static int? _playerScore;
        private static bool? _tutorialState;
        private static string _trackMission;
        private static volatile string _buildVersion;
        // Application.installMode is main-thread only; cached for events sent from other threads.
        private static bool? _storeInstall;

        private static readonly Dictionary<string, object> _balances = new();
        private static readonly Dictionary<string, string> _abCohorts = new();
        private static readonly Dictionary<string, object> _globalAdditionalData = new();
        private static Func<AnalyticsEvent, Dictionary<string, object>> _globalDataCallback;

        private static readonly Dictionary<string, object> _initializationInfo = new();
        private static bool _gameStartedSent;
        private static float _sessionStartTime;

        public static event Action<AnalyticsEvent> OnLogEvent;

        public static void Initialize(params IAnalyticsService[] services)
        {
            _services.Clear();
            _services.AddRange(services);
            _serviceSnapshot = _services.ToArray();
            _sessionStartTime = Time.realtimeSinceStartup;
            _storeInstall = Application.installMode == ApplicationInstallMode.Store;
            // The live-ops dashboard drops users whose build_version differs from app_info.version.
            _buildVersion ??= Application.version;
            AnalyticsLifecycleHook.Ensure();
            SendGameStarted();
        }

        public static void HoldEvents()
        {
            _holdEvents = true;
        }

        public static void ReleaseEvents()
        {
            _holdEvents = false;
            lock (_lock)
            {
                while (_pendingEvents.Count > 0)
                    _pendingEvents.Dequeue()();
            }
        }

        /// <summary>
        /// Sends an event that has no dedicated method. Goes through the same pipeline as every other
        /// event (global params, hold queue, services). Prefer a dedicated method when one fits.
        /// </summary>
        public static void Custom(string eventName, Dictionary<string, object> parameters = null)
        {
            Schedule(eventName, parameters ?? new Dictionary<string, object>());
        }

        public static int GetTotalTimeInApp()
        {
            return (int)(Time.realtimeSinceStartup - _sessionStartTime);
        }

        public static void SetPlayerLevel(int playerLevel) => _playerLevel = playerLevel;
        public static void ClearPlayerLevel() => _playerLevel = null;

        public static void SetPlayerXP(int playerXp) => _playerXp = playerXp;
        public static void ClearPlayerXP() => _playerXp = null;

        public static void SetPlayerScore(int? userScore = null) => _playerScore = userScore;
        public static void ClearPlayerScore() => _playerScore = null;

        public static void SetTutorial(bool tutorialState, string trackMission)
        {
            _tutorialState = tutorialState;
            _trackMission = trackMission;
        }

        public static void SetCurrencyBalance(string type, string name, int balance)
        {
            _balances[$"currency_{type}_{name}"] = balance;
        }

        public static void SetItemBalance(string type, string name, int balance)
        {
            _balances[$"item_{type}_{name}"] = balance;
        }

        public static void SetBuildVersion(string buildVersion) => _buildVersion = buildVersion;

        public static void SetGlobalAdditionalData(string key, object value)
        {
            _globalAdditionalData[key] = value;
        }

        public static void SetGlobalAdditionalData(Dictionary<string, object> map)
        {
            foreach (var kv in map) _globalAdditionalData[kv.Key] = kv.Value;
        }

        public static void SetGlobalAdditionalDataCallback(Func<AnalyticsEvent, Dictionary<string, object>> callback)
        {
            _globalDataCallback = callback;
        }

        public static void ClearAllGlobalData()
        {
            _globalAdditionalData.Clear();
            _balances.Clear();
            _abCohorts.Clear();
            _playerLevel = null;
            _playerXp = null;
            _playerScore = null;
            _tutorialState = null;
            _trackMission = null;
            _buildVersion = null;
        }

        public static void ClearGlobalData(string key) => _globalAdditionalData.Remove(key);

        // Static state survives between tests in one domain.
        internal static void ResetForTests()
        {
            lock (_lock) _pendingEvents.Clear();
            _holdEvents = false;
            _services.Clear();
            _serviceSnapshot = Array.Empty<IAnalyticsService>();
            _storeInstall = null;
            _gameStartedSent = false;
            _initializationInfo.Clear();
            _globalDataCallback = null;
            OnLogEvent = null;
            ClearAllGlobalData();
            ResetAdRevenue();
            lock (_warnedNames) _warnedNames.Clear();
        }

        public static IDictionary<string, object> GetGlobalAdditionalData() => _globalAdditionalData;

        public static void AbCohort(string experimentName, string experimentCohort, Dictionary<string, object> additionalData = null)
        {
            _abCohorts[experimentName] = experimentCohort;
        }

        public static void ClearAbCohort(string experimentName) => _abCohorts.Remove(experimentName);

        public static void AddInitializationInfo(string key, object value)
        {
            if (_gameStartedSent)
            {
                Schedule("debug_initialization_info", new Dictionary<string, object> { { key, value } });
                return;
            }
            _initializationInfo[key] = value;
        }

        private static void SendGameStarted()
        {
            _gameStartedSent = true;
            Schedule("game_started", new Dictionary<string, object>(_initializationInfo));
        }

        internal static void Schedule(string eventName, Dictionary<string, object> parameters)
        {
            Enqueue(() => Dispatch(eventName, parameters));
        }

        // For events that may be raised off the main thread (ad revenue): only build/QA params are added,
        // no player state, balances or cohorts, so the event stays small and nothing touches Unity API.
        internal static void ScheduleLight(string eventName, Dictionary<string, object> parameters)
        {
            Enqueue(() => DispatchLight(eventName, parameters));
        }

        private static void Enqueue(Action fire)
        {
            if (_holdEvents)
            {
                lock (_lock)
                {
                    if (_holdEvents)
                    {
                        _pendingEvents.Enqueue(fire);
                        return;
                    }
                }
            }

            fire();
        }

        private static void Dispatch(string eventName, Dictionary<string, object> parameters)
        {
            var merged = BuildParameters(eventName, parameters);
            Deliver(eventName, merged);
        }

        private static void DispatchLight(string eventName, Dictionary<string, object> parameters)
        {
            parameters["debug"] = DebugFlag;
            var store = _storeInstall;
            if (store.HasValue) parameters["store"] = store.Value;
            var buildVersion = _buildVersion;
            if (buildVersion != null) parameters["build_version"] = buildVersion;
            var level = _playerLevel;
            if (level.HasValue) parameters["user_level"] = level.Value;
            Deliver(eventName, parameters);
        }

        private static void Deliver(string eventName, Dictionary<string, object> parameters)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Validate(eventName, parameters);
#endif
            OnLogEvent?.Invoke(new AnalyticsEvent(eventName, parameters));

            var services = _serviceSnapshot;
            for (int i = 0; i < services.Length; i++)
                services[i].LogEvent(eventName, parameters);
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        private const string DebugFlag = "true";
#else
        private const string DebugFlag = "false";
#endif

        private static readonly Regex ValidName = new("^[a-z][a-z0-9_]{0,39}$");
        private static readonly Regex NumberFragment = new("(^|_)[0-9]+(_|$)");
        private static readonly Regex TutorialStepName = new("^open_tutorial_step_[0-9]+_(start|finish)$");
        private static readonly HashSet<string> ReservedNames = new()
        {
            "ad_reward", "app_exception", "app_remove", "app_update", "error", "first_open",
            "in_app_purchase", "os_update", "screen_view", "session_start", "user_engagement",
        };
        private static readonly HashSet<string> _warnedNames = new();

        // Dev builds only. Warns once per event name; never throws.
        private static void Validate(string eventName, Dictionary<string, object> parameters)
        {
            string problem = null;
            if (eventName == null || !ValidName.IsMatch(eventName))
                problem = "must be snake_case, start with a letter, max 40 chars";
            else if (ReservedNames.Contains(eventName) || eventName.StartsWith("firebase_") || eventName.StartsWith("google_") || eventName.StartsWith("ga_"))
                problem = "is reserved by Firebase and will be dropped";
            else if (NumberFragment.IsMatch(eventName) && !TutorialStepName.IsMatch(eventName))
                problem = "contains a number; put values into parameters (GA4 allows ~500 distinct event names)";
            else if (parameters.Count > 25)
                problem = $"has {parameters.Count} parameters; Firebase keeps only 25";

            if (problem == null) return;
            lock (_warnedNames)
            {
                if (!_warnedNames.Add(eventName ?? string.Empty)) return;
            }
            Debug.LogWarning($"[RaccoonsAnalytics] Event '{eventName}' {problem}.");
        }

        private static Dictionary<string, object> BuildParameters(string eventName, Dictionary<string, object> eventParams)
        {
            var result = new Dictionary<string, object>();

            result["debug"] = DebugFlag;
            result["store"] = _storeInstall ?? Application.installMode == ApplicationInstallMode.Store;

            if (_playerLevel.HasValue) result["user_level"] = _playerLevel.Value;
            if (_playerXp.HasValue) result["user_xp"] = _playerXp.Value;
            if (_playerScore.HasValue) result["user_score"] = _playerScore.Value;
            if (_tutorialState.HasValue) result["user_tutorial"] = _tutorialState.Value;
            if (_buildVersion != null) result["build_version"] = _buildVersion;

            foreach (var kv in _balances) result[kv.Key] = kv.Value;
            foreach (var cohort in _abCohorts) result[$"ab_{cohort.Key}"] = cohort.Value;
            foreach (var kv in _globalAdditionalData) result[kv.Key] = kv.Value;
            foreach (var kv in eventParams) result[kv.Key] = kv.Value;

            if (_globalDataCallback != null)
            {
                var callbackData = _globalDataCallback(new AnalyticsEvent(eventName, result));
                if (callbackData != null)
                    foreach (var kv in callbackData) result[kv.Key] = kv.Value;
            }

            return result;
        }
    }
}
