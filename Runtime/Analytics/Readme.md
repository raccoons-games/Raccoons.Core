# Raccoons Analytics

Provider-agnostic analytics facade for Unity games. Your game code calls strongly-typed
methods on the static `RaccoonsAnalytics` class (`MissionStarted`, `InterstitialShow`,
`InAppPurchase`, …); each call is turned into a named event with a flat parameter dictionary
and dispatched to every registered `IAnalyticsService`. Global player state (level, XP,
currency balances, A/B cohorts) is attached to *every* event automatically, so you set it once
and forget it.

Namespace: `Raccoons.Analytics` (Firebase integration lives in `Raccoons.Analytics.Firebase`).

## Architecture

```
game code ──▶ RaccoonsAnalytics.MissionStarted(...)   (strongly-typed API, partial classes)
                        │
                        ▼
              BuildParameters()  ── merges global state (level, xp, balances, ab, debug, store)
                        │
                        ▼
                 Dispatch(eventName, params)
                   ├──▶ OnLogEvent  (C# event — hook for debug UI / mediation)
                   └──▶ IAnalyticsService.LogEvent(...)  ×N  (Firebase, GameAnalytics, custom…)
```

- **`IAnalyticsService`** — the single integration point. One method:
  `void LogEvent(string eventName, Dictionary<string, object> parameters)`.
- **`RaccoonsAnalytics`** — a `static partial class` split by domain:
  `.cs` (core/global state), `.Mission`, `.Ads`, `.Monetization`, `.InGame`, `.Lifecycle`.
- **Models** (`Product`, `Reward`, `Transaction`, `VirtualCurrency`, `Item`, `RealCurrency`) —
  describe rewards and economy, and know how to flatten themselves into event parameters.

## Quick start

### 1. Register one or more services on startup

```C#
using Raccoons.Analytics;
using Raccoons.Analytics.Firebase;

// Anywhere during app boot (once). Sends `game_started` immediately.
RaccoonsAnalytics.Initialize(new FirebaseAnalyticsService());

// Multiple back-ends at once are supported:
RaccoonsAnalytics.Initialize(new FirebaseAnalyticsService(), new DebugLogAnalyticsService());
```

### 2. Log events from game code

```C#
RaccoonsAnalytics.MissionStarted(missionType: "level", missionName: "level_1", missionID: 1, missionAttempt: 1);
RaccoonsAnalytics.MissionCompleted("level", "level_1", 1, missionAttempt: 1,
    reward: new Reward(new Product {
        VirtualCurrencies = new List<VirtualCurrency> { new VirtualCurrency("coins", "soft", 60) }
    }));
```

That's the whole contract. Everything below is detail.

## Global state (attached to every event)

Set these once (e.g. after loading the player profile); they are merged into the parameters of
**every** subsequent event by `BuildParameters`. Setting is cheap — no event is emitted.

| Call | Parameter key(s) added to events |
|------|----------------------------------|
| `SetPlayerLevel(int)` / `ClearPlayerLevel()` | `user_level` |
| `SetPlayerXP(int)` / `ClearPlayerXP()` | `user_xp` |
| `SetPlayerScore(int?)` / `ClearPlayerScore()` | `user_score` |
| `SetTutorial(bool, string trackMission)` | `user_tutorial` |
| `SetBuildVersion(string)` | `build_version` |
| `SetCurrencyBalance(type, name, balance)` | `currency_{type}_{name}` |
| `SetItemBalance(type, name, balance)` | `item_{type}_{name}` |
| `AbCohort(experimentName, cohort)` | `ab_{experimentName}` |
| `SetGlobalAdditionalData(key, value)` / `(Dictionary)` | `{key}` |
| `SetGlobalAdditionalDataCallback(Func<AnalyticsEvent, Dictionary>)` | dynamic per-event keys |

Always injected automatically:

- `debug` — `"true"` in Editor / development builds, else `"false"`.
- `store` — `bool`, whether the app was installed from a store (`Application.installMode`).

Cleanup: `ClearGlobalData(key)`, `ClearAbCohort(name)`, `ClearAllGlobalData()`,
`GetGlobalAdditionalData()`.

**Precedence** (later overrides earlier): defaults → global state → balances → A/B cohorts →
global additional data → the event's own parameters → the global data callback.

## Lifecycle & event buffering

- `Initialize(params IAnalyticsService[])` — replaces the service list, records the session
  start time, and sends `game_started` (carrying any `AddInitializationInfo` you queued
  beforehand). Call once.
- `AddInitializationInfo(key, value)` — attach data to `game_started`. Must be called
  **before** `Initialize`; afterwards it is sent as a separate `debug_initialization_info` event.
- `HoldEvents()` / `ReleaseEvents()` — queue events while an async SDK is still initializing,
  then flush them in order. This is how `FirebaseAnalyticsInitializer` avoids losing early
  events (see below). Thread-safe.
- `GetTotalTimeInApp()` — seconds since `Initialize`.
- `OnLogEvent` — `event Action<AnalyticsEvent>` fired for every dispatched event *before* the
  services. Use it for an on-screen debug overlay or a mediation adapter.
- `NewPlayer(additionalData?)` — logs `new_player`. `GameStart()` is **obsolete** — a no-op;
  `game_started` is automatic.

## Implementing a custom service

Implement `IAnalyticsService` and forward to your SDK. The parameter dictionary is already
flat and contains only primitive-ish values (`string`, `int`, `long`, `float`, `double`,
`bool`). Reference implementation — `Samples/…/DebugLogAnalyticsService.cs`:

```C#
public class DebugLogAnalyticsService : IAnalyticsService
{
    public void LogEvent(string eventName, Dictionary<string, object> parameters)
    {
        Debug.Log($"[Analytics] {eventName}  {Format(parameters)}");
    }
}
```

Register it via `RaccoonsAnalytics.Initialize(new DebugLogAnalyticsService())`.

## Firebase integration

A ready-made back-end ships in `Runtime/Analytics/Firebase/`.

1. Install the **Firebase Analytics Unity SDK** (`com.google.firebase.analytics`).
2. Open **Raccoons → Integrations**, find *Firebase Analytics*, and click **Enable**. This adds
   the `RACCOONS_FIREBASE_ANALYTICS` scripting define. Without that symbol
   `FirebaseAnalyticsService.LogEvent` compiles to a no-op (safe to keep in code with the SDK
   absent).
3. Either call `Initialize(new FirebaseAnalyticsService())` yourself, or drop a
   **`FirebaseAnalyticsInitializer`** component into your boot scene. The initializer:
   - calls `HoldEvents()` in `Awake` so nothing is lost during async init;
   - runs `FirebaseApp.CheckAndFixDependenciesAsync()`;
   - on success, `Initialize(new FirebaseAnalyticsService())` + `ReleaseEvents()`;
   - on failure, logs an error and leaves analytics disabled.

`FirebaseAnalyticsService` maps values to `Firebase.Analytics.Parameter`: `string`→string,
`int`/`long`→long, `float`/`double`→double, `bool`→`1`/`0`, `null`→empty string, anything
else→`ToString()`.

## Rewards & economy models

Events that grant or move value take model objects that flatten into parameters.

```C#
// A Product bundles what was granted/spent.
var product = new Product {
    VirtualCurrencies = new List<VirtualCurrency> { new VirtualCurrency("coins", "soft", 100) },
    Items            = new List<Item>            { new Item("sword", "weapon", 1) },
    RealCurrency     = new RealCurrency("USD", 4.99f),
};

// Reward = a received Product → keys reward_type/reward_name/reward_amount
//          (reward_1_*, reward_item_0_*, reward_real_* for extras).
var reward = new Reward(product);

// Transaction = named spent+received pair, used by Economy / InAppPurchase.
var tx = new Transaction("shop_buy", spent: cost, received: product,
                         transactionID: "tx-123", productID: "com.game.pack1");
```

`VirtualCurrency(name, type, amount)`, `Item(name, type, amount)`,
`RealCurrency(type, amount)`.

## Event catalogue

Every method also has an `EventArgs` overload (`MissionStarted(MissionEventArgs, …)` etc.) and
an optional `additionalData` dictionary that is merged into that event only.

> **⚠️ Required events.** The following are **mandatory** in every integration — the core
> funnel and monetization dashboards depend on them, and a build is not considered correctly
> instrumented without them. They are marked **(required)** in the tables below.
>
> - **Levels / missions** — `mission_started`, `mission_completed`, `mission_failed`
>   (send `mission_attempt` on each; keep `mission_id` / `mission_name` stable per level).
> - **Boosters / power-ups** — `power_up_used` for every booster the player activates.
> - **Revenue / ads** — the ad *show* events (`interstitial_show`, `reward_video_show`,
>   `banner_show`) **and** the paid-revenue events
>   (`interstitial_revenue_paid`, `reward_video_revenue_paid`, `banner_revenue_paid`).
>   Also send `in_app_purchase` whenever IAP is present.
>
> Everything else in the catalogue is optional and game-specific.

### Missions — `RaccoonsAnalytics.Mission`
Common args: `missionType`, `missionName`, `missionID`, `missionAttempt?`, `isGamePlay?`.

| Method | Event | |
|--------|-------|---|
| `MissionStarted(...)` | `mission_started` | **required** |
| `MissionCompleted(..., Reward reward = null)` | `mission_completed` | **required** |
| `MissionFailed(..., string failReason = null)` | `mission_failed` | **required** |
| `MissionAbandoned(...)` | `mission_abandoned` | optional |
| `MissionStep(..., Reward reward, string stepName)` | `mission_step` | optional |

### Ads — `RaccoonsAnalytics.Ads`
Common args: `placement`, `network` (→ `ad_provider`), `level?` (→ `level_num`).

**required** in every ad-enabled build: `interstitial_show`, `reward_video_show`, `banner_show`
(the impression) **and** `interstitial_revenue_paid`, `reward_video_revenue_paid`,
`banner_revenue_paid` (the paid revenue). The rest of the funnel below is optional.

- **Interstitial:** `interstitial_load`, `interstitial_load_fail`, `interstitial_try_show`,
  `interstitial_show_requested`, **`interstitial_show` (required)**, `interstitial_show_fail`,
  `interstitial_end`, `interstitial_click`, **`interstitial_revenue_paid` (required)**.
- **Rewarded video:** `reward_video_load`, `reward_video_load_fail`, `reward_video_try_show`,
  `reward_video_show_requested`, **`reward_video_show` (required)**, `reward_video_show_fail`,
  `reward_video_end`, `reward_video_click`, `reward_video_collect`, `reward_video_opportunity`,
  **`reward_video_revenue_paid` (required)**.
- **Banner:** `banner_load`, `banner_load_fail`, `banner_show_requested`,
  **`banner_show` (required)**, `banner_show_fail`, `banner_hide`,
  **`banner_revenue_paid` (required)**.

Failures take an `AdErrorType` (`Undefined`, `NoFill`, `NetworkError`, `AdExpired`,
`InvalidRequest`, `InternalError`) → `error_reason`. Revenue methods add a `revenue` (double).

### Monetization — `RaccoonsAnalytics.Monetization`

| Method | Event | |
|--------|-------|---|
| `Economy(Transaction, placement = "General")` / `(name, spent, received, …)` | `economy` | optional |
| `InAppPurchase(Transaction, placement, ReceiptStatus, bool isTestPurchase)` | `in_app_purchase` | **required** (if IAP present) |

`ReceiptStatus`: `NoValidation`, `Success`, `Failure`.

### In-game — `RaccoonsAnalytics.InGame`

**`power_up_used` (required)** — log it for every booster/power-up the player activates
(`PowerUpUsed(missionID, missionType, missionAttempt, powerUpName, missionName)`).

Optional: `item_collected`, `shop_entered`, `achievement`, `ui_interaction`,
`feature_unlocked`, `level_up`, `skill_upgraded`, `skill_used`, `character_created`,
`character_updated`, `character_deleted`, `options`, `gift_sent`, `gift_received`,
`item_actioned`, `product_viewed`, `notification_opened`, `notification_scheduled`,
`notification_cancelled`, `hand_action`.

## Sample

`Samples/Analytics/LevelFlowSample/` is an importable demo: an IMGUI panel
(`AnalyticsLevelFlowSample.cs`) that fires realistic level flows (start → power-up → complete,
soft-fail → ad-revive → complete, checkpoints, abandon, …) through `RaccoonsAnalytics`, plus a
live event log driven by `OnLogEvent`. Open the `AnalyticsLevelFlow` scene and press Play, or
import the sample via the Package Manager.
