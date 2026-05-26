# Lion SDK Analytics — Специфікація API

> Документ описує виключно `LionAnalytics` — статичний клас з пакету
> `com.lionstudios.release.lionanalytics`. Жодного коду гри тут немає.
> При реалізації власного SDK API повинен повністю повторювати цю структуру.

---

## 1. Загальна концепція

`LionAnalytics` — це `partial static` клас у namespace `LionStudios.Suite.Analytics`.
Він реалізує `ILionModule` і ініціалізується автоматично через `LionCore`.

**Ключові особливості:**
- Всі методи — **static**, виклик без екземпляру: `LionAnalytics.MissionStarted(...)`
- Кожна подія потрапляє у **EventScheduler** → `delegate { ev.Fire() }`
- `ev.Fire()` використовує **Dispatcher**, який передає подію в усі підключені SDK-бриджі (Firebase, Adjust, Facebook тощо)
- Є механізм **Hold/Release**: події можна ставити в чергу до готовності SDK

---

## 2. Ініціалізація та lifecycle

```csharp
// LionAnalytics ініціалізується через LionCore — вручну не треба.
// Після ініціалізації автоматично надсилається:
// → game_started (один раз за сесію)

// Якщо потрібно затримати відправку подій (до отримання User ID тощо):
LionAnalytics.HoldEvents();
// ... після готовності:
LionAnalytics.ReleaseEvents(); // звільняє всі накопичені події

// Підписатись на кожну подію (для логування, дебагу):
LionAnalytics.OnLogEvent += (LionGameEventBase e) => { Debug.Log(e.eventName); };
```

`game_started` надсилається один раз при ініціалізації. Метод `GameStart()` — **deprecated**, нічого не робить.

---

## 3. Глобальні параметри (додаються до кожної події)

Ці параметри встановлюються один раз і автоматично потрапляють в payload всіх наступних подій.

### 3.1 Дані гравця

```csharp
// Рівень акаунту гравця (не рівень gameplay)
LionAnalytics.SetPlayerLevel(int playerLevel);
LionAnalytics.ClearPlayerLevel();

// Досвід гравця
LionAnalytics.SetPlayerXP(int playerXp);
LionAnalytics.ClearPlayerXP();

// Рахунок гравця
LionAnalytics.SetPlayerScore(int? userScore = null);
LionAnalytics.ClearPlayerScore();

// Туторіал
LionAnalytics.SetTutorial(bool tutorialState, string trackMission);
```

### 3.2 Валюта та баланси

```csharp
// Встановлює баланс валюти — додається до всіх подій
// type: тип валюти ("soft", "hard")
// name: назва ("coins", "gems")
// balance: поточна кількість
LionAnalytics.SetCurrencyBalance(string type, string name, int balance);

// Те саме для предметів (не валюта)
LionAnalytics.SetItemBalance(string type, string name, int balance);
```

### 3.3 Версія білду

```csharp
LionAnalytics.SetBuildVersion(string buildVersion);
```

### 3.4 Довільні глобальні дані

```csharp
// Додати одне поле — буде у additional_data кожної події
LionAnalytics.SetGlobalAdditionalData(string key, object value);

// Додати словник полів одразу
LionAnalytics.SetGlobalAdditionalData(Dictionary<string, object> map);

// Callback — викликається при кожній події, результат іде в additional_data
// Дозволяє додавати різні дані залежно від типу події
LionAnalytics.SetGlobalAdditionalDataCallback(
    Func<LionGameEventBase, Dictionary<string, object>> callback
);

// Очистити все
LionAnalytics.ClearAllGlobalData();
LionAnalytics.ClearGlobalData(string key);
LionAnalytics.GetGlobalAdditionalData(); // → IDictionary<string, object>
```

### 3.5 Вбудовані глобальні параметри (автоматичні)

Ці параметри встановлюються SDK самостійно і додаються до кожної події:

| Backend key | Значення |
|-------------|---------|
| `debug` | `"true"` якщо `DEVELOPMENT_BUILD`, інакше `"false"` |
| `store` | `true` якщо `Application.installMode == Store` |
| `user_tutorial` | з `SetTutorial()` |
| `user_level` | з `SetPlayerLevel()` |
| `user_xp` | з `SetPlayerXP()` |
| `user_score` | з `SetPlayerScore()` |
| `balances` | з `SetCurrencyBalance()` / `SetItemBalance()` |
| `build_version` | з `SetBuildVersion()` |
| `lslc-version` | версія LionCore (автоматично) |
| `lsla-version` | версія LionAnalytics (автоматично) |

---

## 4. A/B Тести

```csharp
// Прив'язати гравця до когорти — дані додаються до всіх подій
// Naming convention: {game_code}_{platform}_{test.name}
// Приклади: "SNK_ios_start.screen", "SNK_and_rewarded.placement"
LionAnalytics.AbCohort(
    experimentName: string,
    experimentCohort: string,
    additionalData: Dictionary<string, object> = null
);

// Очистити когорту (якщо гравець вийшов з експерименту)
LionAnalytics.ClearAbCohort(string experimentName);
```

---

## 5. Додаткова ініціалізаційна інформація

```csharp
// Додати дані, які потраплять у game_started.additional_data
// Викликати до ініціалізації LionCore (наприклад у [RuntimeInitializeOnLoadMethod])
LionAnalytics.AddInitializationInfo(string key, object value);

// Якщо game_started вже надіслано — автоматично надсилається debug event
// замість додавання до game_started
```

---

## 6. Трекінг часу

```csharp
// Загальний час у застосунку (секунди) — автоматично додається до подій
int seconds = LionAnalytics.GetTotalTimeInApp();
```

---

## 7. Gameplay Events

### `game_started` (автоматично)
Надсилається **один раз** після ініціалізації Lion SDK. Розробнику викликати не потрібно.

```csharp
// [Deprecated] — більше нічого не робить
LionAnalytics.GameStart();
```

---

### `new_player`

```csharp
LionAnalytics.NewPlayer(Dictionary<string, object> additionalData = null);
```

---

## 8. Mission Events

Mission — основна одиниця прогресу. Рівень, задача, квест — все є місією.

### Загальні параметри місій

| Параметр | Backend key | Тип | Обов'язковий |
|----------|-------------|-----|--------------|
| missionType | `mission_type` | string | ні |
| missionName | `mission_name` | string | ні |
| missionID | `mission_id` | **int** | **так** |
| missionAttempt | `mission_attempt` | int? | ні |
| additionalData | `additional_data` | Dictionary | ні |
| isGamePlay | `is_gameplay` | bool? | ні |

> `missionID` **зобов'язаний** парситись як ціле число — рядковий ID не підтримується.

---

### `mission_started`

```csharp
LionAnalytics.MissionStarted(
    missionType: string,
    missionName: string,
    missionID: int,
    missionAttempt: int? = null,
    additionalData: Dictionary<string, object> = null,
    isGamePlay: bool? = null
);

// Альтернатива через EventArgs
LionAnalytics.MissionStarted(MissionEventArgs args, Dictionary<string, object> additionalData = null);
```

---

### `mission_completed`

```csharp
LionAnalytics.MissionCompleted(
    missionType: string,
    missionName: string,
    missionID: int,
    missionAttempt: int? = null,
    additionalData: Dictionary<string, object> = null,
    reward: Reward = null,
    isGamePlay: bool? = null
);

LionAnalytics.MissionCompleted(MissionCompletedEventArgs args, Dictionary<string, object> additionalData = null);
```

---

### `mission_failed`

```csharp
LionAnalytics.MissionFailed(
    missionType: string,
    missionName: string,
    missionID: int,
    missionAttempt: int? = null,
    additionalData: Dictionary<string, object> = null,
    failReason: string = null,       // backend key: fail_reason
    isGamePlay: bool? = null
);

LionAnalytics.MissionFailed(MissionFailedEventArgs args, Dictionary<string, object> additionalData = null);
```

---

### `mission_abandoned`

```csharp
LionAnalytics.MissionAbandoned(
    missionType: string,
    missionName: string,
    missionID: int,
    missionAttempt: int? = null,
    additionalData: Dictionary<string, object> = null,
    isGamePlay: bool? = null
);

LionAnalytics.MissionAbandoned(MissionEventArgs args, Dictionary<string, object> additionalData = null);
```

---

### `mission_step`

Використовується для чекпоінтів та проміжного прогресу всередині місії.

```csharp
LionAnalytics.MissionStep(
    missionType: string,
    missionName: string,
    missionID: int,
    missionAttempt: int? = null,
    additionalData: Dictionary<string, object> = null,
    reward: Reward = null,
    stepName: string = null,         // backend key: step_name
    isGamePlay: bool? = null
);

LionAnalytics.MissionStep(MissionStepEventArgs args, Dictionary<string, object> additionalData = null);
```

---

## 9. In-Game Events

### `item_collected`

```csharp
LionAnalytics.ItemCollected(
    reward: Reward,                                    // обов'язковий
    additionalData: Dictionary<string, object> = null
);
```

---

### `shop_entered`

```csharp
LionAnalytics.ShopEntered(
    shopName: string,                // backend: shop_name — обов'язковий
    shopID: string = null,           // backend: shop_id
    shopType: string = null,         // backend: shop_type
    additionalData: Dictionary<string, object> = null
);
```

---

### `achievement`

```csharp
LionAnalytics.Achievement(
    reward: Reward,                  // обов'язковий
    achievementID: string,           // backend: achievement_id
    achievementName: string,         // backend: achievement_name
    additionalData: Dictionary<string, object> = null
);
```

---

### `power_up_used`

```csharp
LionAnalytics.PowerUpUsed(
    missionID: string,               // backend: mission_id
    missionType: string,             // backend: mission_type
    missionAttempt: int,             // backend: mission_attempt — обов'язковий
    powerUpName: string,             // backend: power_up_name — обов'язковий
    missionName: string = "",        // backend: mission_name
    additionalData: Dictionary<string, object> = null
);
```

---

### `ui_interaction`

```csharp
LionAnalytics.UiInteraction(
    uiAction: string,                // backend: ui_action — обов'язковий
    uiName: string,                  // backend: ui_name — обов'язковий
    uiLocation: string = null,       // backend: ui_location
    uiType: string = null,           // backend: ui_type
    additionalData: Dictionary<string, object> = null
);
```

---

### `feature_unlocked`

```csharp
LionAnalytics.FeatureUnlocked(
    featureName: string,             // backend: feature_name
    featureType: string,             // backend: feature_type
    additionalData: Dictionary<string, object> = null
);
```

---

### `level_up` (рівень акаунту, не gameplay)

```csharp
LionAnalytics.LevelUp(
    levelUpName: string,             // backend: level_up_name
    reward: Reward,
    additionalData: Dictionary<string, object> = null
);
```

---

### `skill_upgraded`

```csharp
LionAnalytics.SkillUpgraded(
    currentSkillLevel: int,
    newSkillLevel: int,
    skillId: string,
    skillName: string,
    additionalData: Dictionary<string, object> = null
);
```

---

### `skill_used`

```csharp
LionAnalytics.SkillUsed(
    skillID: string,
    skillName: string,
    success: bool,
    reasonForFailure: string,
    additionalData: Dictionary<string, object> = null
);
```

---

### `character_created` / `character_updated` / `character_deleted`

```csharp
LionAnalytics.CharacterCreated(
    characterClass: string,
    characterGender: string,
    characterID: string,
    characterName: string,
    additionalData: Dictionary<string, object> = null
);

LionAnalytics.CharacterUpdated(
    characterClass: string,
    characterID: string,
    characterName: string,
    additionalData: Dictionary<string, object> = null
);

LionAnalytics.CharacterDeleted(
    characterClass: string,
    characterID: string,
    characterName: string,
    additionalData: Dictionary<string, object> = null
);
```

---

### `options`

```csharp
LionAnalytics.Options(
    action: string,                  // backend: action
    option: string,                  // backend: option
    additionalData: Dictionary<string, object> = null
);
```

---

### `gift_sent` / `gift_received`

```csharp
LionAnalytics.GiftSent(
    gift: Reward,
    recipientID: string,
    uniqueTracking: string = null,
    additionalData: Dictionary<string, object> = null
);

LionAnalytics.GiftReceived(
    gift: Reward,
    senderID: string,
    giftAccepted: bool = false,
    uniqueTracking: string = null,
    additionalData: Dictionary<string, object> = null
);
```

---

### `item_actioned`

```csharp
LionAnalytics.ItemActioned(
    action: string,
    itemID: string,
    itemName: string,
    itemType: string,
    additionalData: Dictionary<string, object> = null,
    reward: Reward = null
);
```

---

### `product_viewed`

```csharp
LionAnalytics.ProductViewed(
    viewedProductID: string,
    viewedProductName: string,
    additionalData: Dictionary<string, object> = null
);
```

---

### `notification_opened` / `notification_scheduled` / `notification_cancelled`

```csharp
LionAnalytics.NotificationOpened(
    campaignID: int,
    campaignName: string,
    cohortGroup: string,
    cohortID: int,
    cohortName: string,
    communicationSender: string,
    communicationState: string,
    notificationID: int,
    notificationLaunch: string,
    notificationName: string,
    additionalData: Dictionary<string, object> = null
);

LionAnalytics.NotificationScheduled(
    title: string,
    scheduleTimeUtc: DateTime,
    message: string,
    notificationID: string,
    additionalData: Dictionary<string, object> = null
);

LionAnalytics.NotificationCancelled(
    title: string,
    cancelledTimeUtc: DateTime,
    message: string,
    type: string,
    notificationID: string,
    cancelledReason: string = "",
    additionalData: IDictionary<string, object> = null
);
```

---

### `hand_action` (для покерних ігор)

```csharp
LionAnalytics.HandAction(
    amount: int,
    gameID: string,
    handID: string,
    roundAction: string,
    roundName: string,
    additionalData: Dictionary<string, object> = null
);
```

---

## 10. Ad Events

Всі рекламні методи мають дві сигнатури: через окремі параметри і через `EventArgs` об'єкт.

### AdErrorType enum

```
AdErrorType.Undefined
AdErrorType.NoFill
// ... інші типи помилок
```

---

### Interstitial

| Метод | Event name | Ключові параметри |
|-------|-----------|-------------------|
| `InterstitialLoad(placement, network, level?, additionalData)` | `interstitial_load` | placement, ad_provider, level_num |
| `InterstitialLoadFail(network, reason, level?, additionalData)` | `interstitial_load_fail` | ad_provider, error_reason, level_num |
| `InterstitialTryShow(placement, additionalData)` | `interstitial_try_show` | placement |
| `InterstitialShowRequested(placement, network, level?, additionalData)` | `interstitial_show_requested` | placement, ad_provider |
| `InterstitialShow(placement, network, level?, additionalData)` | `interstitial_show` | placement, ad_provider, level_num |
| `InterstitialShowFail(placement, network, level?, reason, additionalData)` | `interstitial_show_fail` | placement, ad_provider, error_reason |
| `InterstitialEnd(placement, network, level?, additionalData)` | `interstitial_end` | placement, ad_provider |
| `InterstitialClick(placement, network, level?, additionalData)` | `interstitial_click` | placement, ad_provider |
| `InterstitialRevenuePaid(revenue, placement, network, level?, additionalData)` | `interstitial_revenue_paid` | revenue, placement, ad_provider |

```csharp
// Приклади
LionAnalytics.InterstitialLoad(placement: "WinScreen", network: "applovin", level: 25);
LionAnalytics.InterstitialLoadFail(network: "applovin", reason: AdErrorType.NoFill, level: 25);
LionAnalytics.InterstitialTryShow(placement: "WinScreen");
LionAnalytics.InterstitialShow(placement: "WinScreen", network: "applovin", level: 25);
LionAnalytics.InterstitialRevenuePaid(revenue: 0.99d, placement: "WinScreen", network: "applovin");

// Через EventArgs
LionAnalytics.InterstitialLoad(AdEventArgs args, additionalData);
LionAnalytics.InterstitialLoadFail(AdFailEventArgs args, additionalData);
LionAnalytics.InterstitialRevenuePaid(AdRevenueEventArgs args, additionalData);
```

---

### Rewarded Video

| Метод | Event name | Ключові параметри |
|-------|-----------|-------------------|
| `RewardVideoLoad(placement, network, level?, additionalData)` | `reward_video_load` | placement, ad_provider |
| `RewardVideoLoadFail(network, level?, reason, placement, additionalData)` | `reward_video_load_fail` | ad_provider, error_reason |
| `RewardVideoTryShow(placement, additionalData)` | `reward_video_try_show` | placement |
| `RewardVideoShowRequested(placement, network, level?, additionalData)` | `reward_video_show_requested` | placement, ad_provider |
| `RewardVideoShow(placement, network, level?, additionalData)` | `reward_video_show` | placement, ad_provider |
| `RewardVideoShowFail(placement, network, reason, level?, additionalData)` | `reward_video_show_fail` | placement, ad_provider, error_reason |
| `RewardVideoEnd(placement, network, level?, additionalData)` | `reward_video_end` | placement, ad_provider |
| `RewardVideoClick(placement, network, level?, additionalData)` | `reward_video_click` | placement, ad_provider |
| `RewardVideoCollect(placement, reward?, level?, additionalData)` | `reward_video_collect` | placement, reward |
| `RewardVideoOpportunity(placement, additionalData)` | `reward_video_opportunity` | placement |
| `RewardVideoRevenuePaid(revenue, placement, network, level?, additionalData)` | `reward_video_revenue_paid` | revenue, placement, ad_provider |

```csharp
// Приклади
LionAnalytics.RewardVideoLoad(placement: "WinScreen", network: "applovin", level: 25);
LionAnalytics.RewardVideoLoadFail(network: "applovin", level: 25, reason: AdErrorType.NoFill, placement: "winscreen");
LionAnalytics.RewardVideoTryShow(placement: "WinScreen");
LionAnalytics.RewardVideoCollect(placement: "WinScreen", reward: reward, level: 25);
LionAnalytics.RewardVideoRevenuePaid(revenue: 0.99d, placement: "WinScreen", network: "applovin");

// Через EventArgs
LionAnalytics.RewardVideoLoad(AdEventArgs args, additionalData);
LionAnalytics.RewardVideoCollect(AdRewardArgs args, additionalData);
LionAnalytics.RewardVideoRevenuePaid(AdRevenueEventArgs args, additionalData);
```

---

### Banner

| Метод | Event name | Ключові параметри |
|-------|-----------|-------------------|
| `BannerLoad(placement, network, additionalData)` | `banner_load` | placement, ad_provider |
| `BannerLoadFail(network, reason, additionalData)` | `banner_load_fail` | ad_provider, error_reason |
| `BannerShowRequested(placement, network, additionalData)` | `banner_show_requested` | placement, ad_provider |
| `BannerShow(placement, network, additionalData)` | `banner_show` | placement, ad_provider |
| `BannerShowFail(placement, network, reason, additionalData)` | `banner_show_fail` | placement, ad_provider, error_reason |
| `BannerHide(placement, network, additionalData)` | `banner_hide` | placement, ad_provider |
| `BannerRevenuePaid(revenue, placement, network, additionalData)` | `banner_revenue_paid` | revenue, placement, ad_provider |

```csharp
LionAnalytics.BannerLoad(placement: "BaseBanner", network: "applovin");
LionAnalytics.BannerRevenuePaid(revenue: 0.99d, placement: "BaseBanner", network: "applovin");

// Через EventArgs
LionAnalytics.BannerLoad(BannerEventArgs args, additionalData);
LionAnalytics.BannerLoadFail(BannerFailEventArgs args, additionalData);
```

> **Thread-safe events** (надсилаються з будь-якого потоку):
> `banner_revenue_paid`, `interstitial_revenue_paid`, `reward_video_revenue_paid`,
> `banner_show`, `interstitial_show`, `reward_video_show`

---

## 11. Monetization Events

### `economy`

```csharp
// Через Transaction об'єкт (повний контроль)
LionAnalytics.Economy(
    transaction: Transaction,
    placement: string = "General",
    additionalData: Dictionary<string, object> = null
);

// Спрощений варіант
LionAnalytics.Economy(
    transactionName: string,
    spent: Product,
    received: Product,
    placement: string = "General",
    additionalData: Dictionary<string, object> = null
);
```

### `in_app_purchase`

```csharp
// Повний варіант
LionAnalytics.InAppPurchase(
    purchaseName: string,
    spentProducts: Product,
    receivedProducts: Product,
    productID: string,         // Store SKU, напр. "com.studio.game.money1500"
    transactionID: string,     // Receipt ID від Store
    placement: string,
    receiptStatus: ReceiptStatus,
    isTestPurchase: bool,
    additionalData: Dictionary<string, object> = null
);

// Через Transaction об'єкт
LionAnalytics.InAppPurchase(
    transaction: Transaction,
    placement: string,
    receiptStatus: ReceiptStatus,
    isTestPurchase: bool,
    additionalData: Dictionary<string, object> = null
);
```

---

## 12. Моделі даних

### `Reward`

```csharp
// Нагорода гравцю — передається в Mission, Achievement, ItemCollected тощо
var product = new Product();
product.virtualCurrencies = new List<VirtualCurrency>
{
    new VirtualCurrency(type: "coins", name: "gold", amount: 100)
};
var reward = new Reward(product);
```

### `Product`

```csharp
// Те, що витрачено або отримано
var product = new Product();
// Варіанти заповнення:
product.realCurrency = new RealCurrency(type: "$", amount: 25.99f);
product.virtualCurrencies = new List<VirtualCurrency> { ... };
product.items = new List<Item> { ... };
```

### `VirtualCurrency`

```csharp
new VirtualCurrency(name: "gold", type: "coins", amount: 100);
```

### `RealCurrency`

```csharp
new RealCurrency(type: "USD", amount: 4.99f);
```

### `Transaction`

```csharp
new Transaction(
    transactionName: "StarterPack",
    currency: "USD",           // optional
    received: productReceived,
    spent: productSpent,
    transactionID: "xcf22f89574u45k6jy8",  // receipt ID
    productID: "com.studio.game.starterpack"
);

// Можна додавати окремо
transaction.AddSpentItem(name: "USD", type: "real", amount: 4);
transaction.AddReceivedItem(name: "coins", type: "virtual", amount: 1500);
```

### `ReceiptStatus` enum

```
ReceiptStatus.NoValidation
ReceiptStatus.Success
ReceiptStatus.Failure
```

### `MissionEventArgs`

```csharp
new MissionEventArgs
{
    MissionType    = "main",
    MissionName    = "main_5",
    MissionID      = 5,
    MissionAttempt = 3,
    IsGamePlay     = true
}
```

---

## 13. Внутрішній механізм відправки подій

```
LionAnalytics.MissionStarted(...)
    │
    ▼
_eventScheduler.Schedule(delegate {
    var ev = new MissionStartedEvent(params, isCalledExplicitly: false);
    ev.Fire();
})
    │
    ├─ (якщо holdEvents == true) → подія ставиться в чергу
    └─ (якщо holdEvents == false) →
            │
            ▼
        LionGameEventBase.Fire()
            │
            ▼
        Dispatcher → AnalyticsSdkBridge
            ├──► Firebase bridge → FirebaseAnalytics.LogEvent()
            ├──► Adjust bridge  → Adjust.trackEvent()
            └──► інші підключені SDK
```

**EventScheduler** реагує на `ReleaseEvents()` і надсилає всі накопичені події.

---

## 14. Як Lion Studio переглядає події у браузері

### Firebase DebugView (real-time, для QA/розробки)

Щоб бачити події у реальному часі на конкретному пристрої:

```bash
# Android — встановити debug mode
adb shell setprop debug.firebase.analytics.app YOUR_PACKAGE_NAME

# Скинути
adb shell setprop debug.firebase.analytics.app .none.
```

Для iOS — встановити launch argument `-FIRAnalyticsDebugEnabled` в Xcode scheme.

Де дивитись: `Firebase Console → Analytics → DebugView`

- Затримка ~30 секунд
- Видно кожну подію, всі її параметри, timestamp
- Прив'язано до конкретного пристрою (не агрегат)

---

### Firebase Console → Events (агрегат, продакшн)

`Firebase Console → Analytics → Events`

- Затримка 24–48 годин
- Загальна статистика по кожній події
- Кількість, унікальні користувачі, тренди по часу

---

### Firebase BigQuery Export (аналіз сирих даних)

Firebase автоматично стрімить всі події в BigQuery після увімкнення експорту в Console.

```
App → Firebase SDK → Firebase Analytics
    → (BigQuery Export)
    → Dataset: analytics_XXXXXXXXX
        └── Table: events_YYYYMMDD  (партиція per day)
```

Структура рядка в BigQuery:

```sql
SELECT
    event_name,
    event_timestamp,
    user_pseudo_id,
    -- параметри зберігаються як ARRAY of STRUCT {key, value}
    (SELECT value.string_value FROM UNNEST(event_params) WHERE key = 'mission_type')    AS mission_type,
    (SELECT value.int_value    FROM UNNEST(event_params) WHERE key = 'mission_id')      AS mission_id,
    (SELECT value.string_value FROM UNNEST(event_params) WHERE key = 'mission_name')   AS mission_name,
    (SELECT value.int_value    FROM UNNEST(event_params) WHERE key = 'mission_attempt') AS mission_attempt,
    (SELECT value.string_value FROM UNNEST(event_params) WHERE key = 'fail_reason')    AS fail_reason
FROM `project.analytics_XXXXXXXXX.events_20260512`
WHERE event_name = 'mission_failed'
ORDER BY event_timestamp DESC
```

Поверх BigQuery будуються:
- **Looker Studio** (Google Data Studio) — безкоштовний дашборд
- **Tableau / Power BI** — корпоративна аналітика
- Власні Python/pandas ноутбуки для ad-hoc аналізу

---

## 15. Повний список event names (backend)

| Category | Event name |
|----------|-----------|
| Lifecycle | `game_started`, `new_player` |
| Mission | `mission_started`, `mission_completed`, `mission_failed`, `mission_abandoned`, `mission_step` |
| In-Game | `item_collected`, `shop_entered`, `achievement`, `power_up_used`, `ui_interaction`, `feature_unlocked`, `level_up`, `skill_upgraded`, `skill_used`, `character_created`, `character_updated`, `character_deleted`, `options`, `gift_sent`, `gift_received`, `item_actioned`, `product_viewed`, `hand_action` |
| Notifications | `notification_opened`, `notification_scheduled`, `notification_cancelled` |
| Monetization | `economy`, `in_app_purchase` |
| Rewarded | `reward_video_load`, `reward_video_load_fail`, `reward_video_try_show`, `reward_video_show_requested`, `reward_video_show`, `reward_video_show_fail`, `reward_video_end`, `reward_video_click`, `reward_video_collect`, `reward_video_opportunity`, `reward_video_revenue_paid` |
| Interstitial | `interstitial_load`, `interstitial_load_fail`, `interstitial_try_show`, `interstitial_show_requested`, `interstitial_show`, `interstitial_show_fail`, `interstitial_end`, `interstitial_click`, `interstitial_revenue_paid` |
| Banner | `banner_load`, `banner_load_fail`, `banner_show_requested`, `banner_show`, `banner_show_fail`, `banner_hide`, `banner_revenue_paid` |
