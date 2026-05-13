using System;
using System.Collections.Generic;

namespace Raccoons.Analytics
{
    public static partial class RaccoonsAnalytics
    {
        public static void ItemCollected(Reward reward, Dictionary<string, object> additionalData = null)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            MergeParams(p, reward.ToParameters());
            Schedule("item_collected", p);
        }

        public static void ShopEntered(
            string shopName,
            string shopID = null,
            string shopType = null,
            Dictionary<string, object> additionalData = null)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            p["shop_name"] = shopName;
            if (shopID != null) p["shop_id"] = shopID;
            if (shopType != null) p["shop_type"] = shopType;
            Schedule("shop_entered", p);
        }

        public static void Achievement(
            Reward reward,
            string achievementID,
            string achievementName,
            Dictionary<string, object> additionalData = null)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            p["achievement_id"] = achievementID;
            p["achievement_name"] = achievementName;
            MergeParams(p, reward.ToParameters());
            Schedule("achievement", p);
        }

        public static void PowerUpUsed(
            string missionID,
            string missionType,
            int missionAttempt,
            string powerUpName,
            string missionName = "",
            Dictionary<string, object> additionalData = null)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            p["mission_id"] = missionID;
            p["mission_type"] = missionType;
            p["mission_attempt"] = missionAttempt;
            p["power_up_name"] = powerUpName;
            p["mission_name"] = missionName;
            Schedule("power_up_used", p);
        }

        public static void UiInteraction(
            string uiAction,
            string uiName,
            string uiLocation = null,
            string uiType = null,
            Dictionary<string, object> additionalData = null)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            p["ui_action"] = uiAction;
            p["ui_name"] = uiName;
            if (uiLocation != null) p["ui_location"] = uiLocation;
            if (uiType != null) p["ui_type"] = uiType;
            Schedule("ui_interaction", p);
        }

        public static void FeatureUnlocked(
            string featureName,
            string featureType,
            Dictionary<string, object> additionalData = null)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            p["feature_name"] = featureName;
            p["feature_type"] = featureType;
            Schedule("feature_unlocked", p);
        }

        public static void LevelUp(
            string levelUpName,
            Reward reward,
            Dictionary<string, object> additionalData = null)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            p["level_up_name"] = levelUpName;
            if (reward != null) MergeParams(p, reward.ToParameters());
            Schedule("level_up", p);
        }

        public static void SkillUpgraded(
            int currentSkillLevel,
            int newSkillLevel,
            string skillId,
            string skillName,
            Dictionary<string, object> additionalData = null)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            p["current_skill_level"] = currentSkillLevel;
            p["new_skill_level"] = newSkillLevel;
            p["skill_id"] = skillId;
            p["skill_name"] = skillName;
            Schedule("skill_upgraded", p);
        }

        public static void SkillUsed(
            string skillID,
            string skillName,
            bool success,
            string reasonForFailure,
            Dictionary<string, object> additionalData = null)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            p["skill_id"] = skillID;
            p["skill_name"] = skillName;
            p["success"] = success;
            p["reason_for_failure"] = reasonForFailure;
            Schedule("skill_used", p);
        }

        public static void CharacterCreated(
            string characterClass,
            string characterGender,
            string characterID,
            string characterName,
            Dictionary<string, object> additionalData = null)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            p["character_class"] = characterClass;
            p["character_gender"] = characterGender;
            p["character_id"] = characterID;
            p["character_name"] = characterName;
            Schedule("character_created", p);
        }

        public static void CharacterUpdated(
            string characterClass,
            string characterID,
            string characterName,
            Dictionary<string, object> additionalData = null)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            p["character_class"] = characterClass;
            p["character_id"] = characterID;
            p["character_name"] = characterName;
            Schedule("character_updated", p);
        }

        public static void CharacterDeleted(
            string characterClass,
            string characterID,
            string characterName,
            Dictionary<string, object> additionalData = null)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            p["character_class"] = characterClass;
            p["character_id"] = characterID;
            p["character_name"] = characterName;
            Schedule("character_deleted", p);
        }

        public static void Options(
            string action,
            string option,
            Dictionary<string, object> additionalData = null)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            p["action"] = action;
            p["option"] = option;
            Schedule("options", p);
        }

        public static void GiftSent(
            Reward gift,
            string recipientID,
            string uniqueTracking = null,
            Dictionary<string, object> additionalData = null)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            p["recipient_id"] = recipientID;
            if (uniqueTracking != null) p["unique_tracking"] = uniqueTracking;
            MergeParams(p, gift.ToParameters("gift"));
            Schedule("gift_sent", p);
        }

        public static void GiftReceived(
            Reward gift,
            string senderID,
            bool giftAccepted = false,
            string uniqueTracking = null,
            Dictionary<string, object> additionalData = null)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            p["sender_id"] = senderID;
            p["gift_accepted"] = giftAccepted;
            if (uniqueTracking != null) p["unique_tracking"] = uniqueTracking;
            MergeParams(p, gift.ToParameters("gift"));
            Schedule("gift_received", p);
        }

        public static void ItemActioned(
            string action,
            string itemID,
            string itemName,
            string itemType,
            Dictionary<string, object> additionalData = null,
            Reward reward = null)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            p["action"] = action;
            p["item_id"] = itemID;
            p["item_name"] = itemName;
            p["item_type"] = itemType;
            if (reward != null) MergeParams(p, reward.ToParameters());
            Schedule("item_actioned", p);
        }

        public static void ProductViewed(
            string viewedProductID,
            string viewedProductName,
            Dictionary<string, object> additionalData = null)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            p["viewed_product_id"] = viewedProductID;
            p["viewed_product_name"] = viewedProductName;
            Schedule("product_viewed", p);
        }

        public static void NotificationOpened(
            int campaignID,
            string campaignName,
            string cohortGroup,
            int cohortID,
            string cohortName,
            string communicationSender,
            string communicationState,
            int notificationID,
            string notificationLaunch,
            string notificationName,
            Dictionary<string, object> additionalData = null)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            p["campaign_id"] = campaignID;
            p["campaign_name"] = campaignName;
            p["cohort_group"] = cohortGroup;
            p["cohort_id"] = cohortID;
            p["cohort_name"] = cohortName;
            p["communication_sender"] = communicationSender;
            p["communication_state"] = communicationState;
            p["notification_id"] = notificationID;
            p["notification_launch"] = notificationLaunch;
            p["notification_name"] = notificationName;
            Schedule("notification_opened", p);
        }

        public static void NotificationScheduled(
            string title,
            DateTime scheduleTimeUtc,
            string message,
            string notificationID,
            Dictionary<string, object> additionalData = null)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            p["title"] = title;
            p["schedule_time_utc"] = scheduleTimeUtc.ToString("O");
            p["message"] = message;
            p["notification_id"] = notificationID;
            Schedule("notification_scheduled", p);
        }

        public static void NotificationCancelled(
            string title,
            DateTime cancelledTimeUtc,
            string message,
            string type,
            string notificationID,
            string cancelledReason = "",
            Dictionary<string, object> additionalData = null)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            p["title"] = title;
            p["cancelled_time_utc"] = cancelledTimeUtc.ToString("O");
            p["message"] = message;
            p["type"] = type;
            p["notification_id"] = notificationID;
            p["cancelled_reason"] = cancelledReason;
            Schedule("notification_cancelled", p);
        }

        public static void HandAction(
            int amount,
            string gameID,
            string handID,
            string roundAction,
            string roundName,
            Dictionary<string, object> additionalData = null)
        {
            var p = additionalData != null ? new Dictionary<string, object>(additionalData) : new Dictionary<string, object>();
            p["amount"] = amount;
            p["game_id"] = gameID;
            p["hand_id"] = handID;
            p["round_action"] = roundAction;
            p["round_name"] = roundName;
            Schedule("hand_action", p);
        }
    }
}
