using System.Collections.Generic;

namespace Raccoons.Analytics
{
    public static partial class RaccoonsAnalytics
    {
        public static void MissionStarted(
            string missionType,
            string missionName,
            int missionID,
            int? missionAttempt = null,
            Dictionary<string, object> additionalData = null,
            bool? isGamePlay = null)
        {
            Schedule("mission_started", BuildMissionParams(missionType, missionName, missionID, missionAttempt, additionalData, isGamePlay));
        }

        public static void MissionStarted(MissionEventArgs args, Dictionary<string, object> additionalData = null)
        {
            MissionStarted(args.MissionType, args.MissionName, args.MissionID, args.MissionAttempt, additionalData, args.IsGamePlay);
        }

        public static void MissionCompleted(
            string missionType,
            string missionName,
            int missionID,
            int? missionAttempt = null,
            Dictionary<string, object> additionalData = null,
            Reward reward = null,
            bool? isGamePlay = null)
        {
            var p = BuildMissionParams(missionType, missionName, missionID, missionAttempt, additionalData, isGamePlay);
            if (reward != null) MergeParams(p, reward.ToParameters());
            Schedule("mission_completed", p);
        }

        public static void MissionCompleted(MissionCompletedEventArgs args, Dictionary<string, object> additionalData = null)
        {
            MissionCompleted(args.MissionType, args.MissionName, args.MissionID, args.MissionAttempt, additionalData, args.Reward, args.IsGamePlay);
        }

        public static void MissionFailed(
            string missionType,
            string missionName,
            int missionID,
            int? missionAttempt = null,
            Dictionary<string, object> additionalData = null,
            string failReason = null,
            bool? isGamePlay = null)
        {
            var p = BuildMissionParams(missionType, missionName, missionID, missionAttempt, additionalData, isGamePlay);
            if (failReason != null) p["fail_reason"] = failReason;
            Schedule("mission_failed", p);
        }

        public static void MissionFailed(MissionFailedEventArgs args, Dictionary<string, object> additionalData = null)
        {
            MissionFailed(args.MissionType, args.MissionName, args.MissionID, args.MissionAttempt, additionalData, args.FailReason, args.IsGamePlay);
        }

        public static void MissionAbandoned(
            string missionType,
            string missionName,
            int missionID,
            int? missionAttempt = null,
            Dictionary<string, object> additionalData = null,
            bool? isGamePlay = null)
        {
            Schedule("mission_abandoned", BuildMissionParams(missionType, missionName, missionID, missionAttempt, additionalData, isGamePlay));
        }

        public static void MissionAbandoned(MissionEventArgs args, Dictionary<string, object> additionalData = null)
        {
            MissionAbandoned(args.MissionType, args.MissionName, args.MissionID, args.MissionAttempt, additionalData, args.IsGamePlay);
        }

        public static void MissionStep(
            string missionType,
            string missionName,
            int missionID,
            int? missionAttempt = null,
            Dictionary<string, object> additionalData = null,
            Reward reward = null,
            string stepName = null,
            bool? isGamePlay = null)
        {
            var p = BuildMissionParams(missionType, missionName, missionID, missionAttempt, additionalData, isGamePlay);
            if (stepName != null) p["step_name"] = stepName;
            if (reward != null) MergeParams(p, reward.ToParameters());
            Schedule("mission_step", p);
        }

        public static void MissionStep(MissionStepEventArgs args, Dictionary<string, object> additionalData = null)
        {
            MissionStep(args.MissionType, args.MissionName, args.MissionID, args.MissionAttempt, additionalData, args.Reward, args.StepName, args.IsGamePlay);
        }

        private static Dictionary<string, object> BuildMissionParams(
            string missionType,
            string missionName,
            int missionID,
            int? missionAttempt,
            Dictionary<string, object> additionalData,
            bool? isGamePlay)
        {
            var p = new Dictionary<string, object>
            {
                ["mission_type"] = missionType,
                ["mission_name"] = missionName,
                ["mission_id"] = missionID
            };
            if (missionAttempt.HasValue) p["mission_attempt"] = missionAttempt.Value;
            if (isGamePlay.HasValue) p["is_gameplay"] = isGamePlay.Value;
            if (additionalData != null) MergeParams(p, additionalData);
            return p;
        }

        private static void MergeParams(Dictionary<string, object> target, Dictionary<string, object> source)
        {
            foreach (var kv in source) target[kv.Key] = kv.Value;
        }
    }
}
