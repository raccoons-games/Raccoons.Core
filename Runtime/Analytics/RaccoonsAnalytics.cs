using System;
using System.Collections.Generic;
using UnityEngine;

namespace Raccoons.Analytics
{
    public static partial class RaccoonsAnalytics
    {
        private static readonly List<IAnalyticsService> _services = new();
        private static readonly Queue<Action> _pendingEvents = new();
        private static readonly object _lock = new();
        private static volatile bool _holdEvents;

        private static int? _playerLevel;
        private static int? _playerXp;
        private static int? _playerScore;
        private static bool? _tutorialState;
        private static string _trackMission;
        private static string _buildVersion;

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
            _sessionStartTime = Time.realtimeSinceStartup;
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
            void Fire() => Dispatch(eventName, parameters);

            if (_holdEvents)
            {
                lock (_lock)
                {
                    if (_holdEvents)
                    {
                        _pendingEvents.Enqueue(Fire);
                        return;
                    }
                }
            }

            Fire();
        }

        private static void Dispatch(string eventName, Dictionary<string, object> parameters)
        {
            var merged = BuildParameters(eventName, parameters);
            var analyticsEvent = new AnalyticsEvent(eventName, merged);

            OnLogEvent?.Invoke(analyticsEvent);

            for (int i = 0; i < _services.Count; i++)
                _services[i].LogEvent(eventName, merged);
        }

        private static Dictionary<string, object> BuildParameters(string eventName, Dictionary<string, object> eventParams)
        {
            var result = new Dictionary<string, object>();

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            result["debug"] = "true";
#else
            result["debug"] = "false";
#endif
            result["store"] = Application.installMode == ApplicationInstallMode.Store;

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
