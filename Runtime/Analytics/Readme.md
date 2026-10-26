# Raccoons Analytics

Provider-agnostic analytics facade for Unity games. Your game code calls strongly-typed
methods on the static `RaccoonsAnalytics` class (`MissionStarted`, `AdImpression`,
`CoinsDelta`, …); each call is turned into a named event with a flat parameter dictionary
and dispatched to every registered `IAnalyticsService`. Global player state (level, XP,
currency balances, A/B cohorts) is attached to every event automatically, so you set it once
and forget it.

Namespace: `Raccoons.Analytics` (Firebase integration lives in `Raccoons.Analytics.Firebase`,
the AppLovin MAX bridge in `Raccoons.Analytics.AppLovinMax`).

> **Connecting a game to the live-ops dashboard** (which events, how many per day, how to
> verify): `Documentation~/Analytics/liveops-analytics-setup.md` (Ukrainian, for developers) and
> `Documentation~/Analytics/RACCOONS_ANALYTICS_AI_SETUP.md` (English, hand it to your AI
> assistant). The event contract itself is owned by rg-liveops (`docs/analytics-event-contract.md`).

## Architecture

```
game code ──▶ RaccoonsAnalytics.MissionStarted(...)   (strongly-typed API, partial classes)
                        │
                        ▼
              BuildParameters()  ── merges global state (level, xp, balances, ab, debug, store, build_version)
                        │
                        ▼
                 Dispatch(eventName, params)
                   ├──▶ OnLogEvent  (C# event — hook for debug UI / mediation)
                   └──▶ IAnalyticsService.LogEvent(...)  ×N  (Firebase, GameAnalytics, custom…)
```

- **`IAnalyticsService`** — the single integration point. One method:
  `void LogEvent(string eventName, Dictionary<string, object> parameters)`.
- **`RaccoonsAnalytics`** — a `static partial class` split by domain:
  `.cs` (core/global state, `Custom`), `.Mission`, `.Ads`, `.AdRevenue`, `.LiveOps`,
  `.Monetization`, `.InGame`, `.Lifecycle`.
- **`FilteredAnalyticsService`** — decorator that drops events nobody reads (see *Event budget*).
- **Models** (`Product`, `Reward`, `Transaction`, `VirtualCurrency`, `Item`, `RealCurrency`) —
  describe rewards and economy, and know how to flatten themselves into event parameters.

## Quick start

### 1. Register services on startup

```C#
using Raccoons.Analytics;
using Raccoons.Analytics.Firebase;

// Once, during app boot, after Firebase dependencies are resolved. Sends `game_started`.
RaccoonsAnalytics.Initialize(new FilteredAnalyticsService(new FirebaseAnalyticsService()));

// Multiple back-ends at once are supported:
RaccoonsAnalytics.Initialize(new FirebaseAnalyticsService(), new DebugLogAnalyticsService());
```

Or drop a `FirebaseAnalyticsInitializer` into the boot scene (see *Firebase integration*).

### 2. Ad revenue (AppLovin MAX)

```C#
using Raccoons.Analytics.AppLovinMax;

MaxSdk.InitializeSdk();
MaxAnalyticsBridge.Attach();   // ad_impression for every format + initial_ecpm
```

### 3. Log events from game code

```C#
RaccoonsAnalytics.SetPlayerLevel(1);
RaccoonsAnalytics.MissionStarted(missionType: "level", missionName: "level_1", missionID: 1, missionAttempt: 1);
RaccoonsAnalytics.MissionCompleted("level", "level_1", 1, missionAttempt: 1,
    reward: new Reward(new Product {
        VirtualCurrencies = new List<VirtualCurrency> { new VirtualCurrency("coins", "soft", 60) }
    }));
RaccoonsAnalytics.CoinsDelta(+60, "level_complete");
```

## Global state (attached to every event)

Set these once (e.g. after loading the player profile); they are merged into the parameters of
**every** subsequent event by `BuildParameters`. Setting is cheap — no event is emitted.

| Call | Parameter key(s) added to events |
|------|----------------------------------|
| `SetPlayerLevel(int)` / `ClearPlayerLevel()` | `user_level` |
| `SetPlayerXP(int)` / `ClearPlayerXP()` | `user_xp` |
| `SetPlayerScore(int?)` / `ClearPlayerScore()` | `user_score` |
| `SetTutorial(bool, string trackMission)` | `user_tutorial` |
| `SetBuildVersion(string)` | `build_version` (defaults to `Application.version` on `Initialize`) |
| `SetCurrencyBalance(type, name, balance)` | `currency_{type}_{name}` |
| `SetItemBalance(type, name, balance)` | `item_{type}_{name}` |
| `AbCohort(experimentName, cohort)` | `ab_{experimentName}` |
| `SetGlobalAdditionalData(key, value)` / `(Dictionary)` | `{key}` |
| `SetGlobalAdditionalDataCallback(Func<AnalyticsEvent, Dictionary>)` | dynamic per-event keys |

Always injected automatically:

- `debug` — `"true"` in Editor / development builds, else `"false"`.
- `store` — `bool`, whether the app was installed from a store (`Application.installMode`,
  cached on `Initialize`).

`build_version` must equal `Application.version`: the live-ops dashboard drops users whose
`build_version` differs from GA4's `app_info.version`.

Cleanup: `ClearGlobalData(key)`, `ClearAbCohort(name)`, `ClearAllGlobalData()`,
`GetGlobalAdditionalData()`.

**Precedence** (later overrides earlier): defaults → global state → balances → A/B cohorts →
global additional data → the event's own parameters → the global data callback.

Firebase keeps at most 25 parameters per event. Balances, cohorts and global data are added to
every event, so keep them few; development builds warn when an event exceeds 25.

## Lifecycle & event buffering

- `Initialize(params IAnalyticsService[])` — replaces the service list, records the session
  start time, caches install mode, and sends `game_started` (carrying any
  `AddInitializationInfo` you queued beforehand). Call once — every call sends another
  `game_started`.
- `AddInitializationInfo(key, value)` — attach data to `game_started`. Must be called
  **before** `Initialize`; afterwards it is sent as a separate `debug_initialization_info` event.
- `HoldEvents()` / `ReleaseEvents()` — queue events while an async SDK is still initializing,
  then flush them in order. Always release in a `finally`. Thread-safe.
- `GetTotalTimeInApp()` — seconds since `Initialize`.
- `OnLogEvent` — `event Action<AnalyticsEvent>` fired for every dispatched event *before* the
  services. It fires even for events a `FilteredAnalyticsService` drops and even when the
  Firebase define is missing, so a "sent" log proves nothing — verify in Firebase DebugView.
  For ad revenue it may fire off the main thread.
- `NewPlayer(additionalData?)` — logs `new_player`. `GameStart()` is **obsolete** — a no-op;
  `game_started` is automatic.
- Development builds warn (once per name, never throw) on event names that are not snake_case,
  longer than 40 chars, reserved by Firebase, or contain a number (`level_12_start`) — GA4
  allows ~500 distinct names, put values into parameters.

## Implementing a custom service

Implement `IAnalyticsService` and forward to your SDK. The parameter dictionary is already
flat and contains only primitive-ish values (`string`, `int`, `long`, `float`, `double`,
`bool`). Reference implementation — `Samples/Analytics/LevelFlowSample/Scripts/DebugLogAnalyticsService.cs`:

```C#
public class DebugLogAnalyticsService : IAnalyticsService
{
    public void LogEvent(string eventName, Dictionary<string, object> parameters)
    {
        Debug.Log($"[Analytics] {eventName}  {Format(parameters)}");
    }
}
```

`LogEvent` may be called off the main thread for ad revenue events.

## Firebase integration

A ready-made back-end ships in `Runtime/Analytics/Firebase/`.

1. Install the **Firebase Analytics Unity SDK** (`com.google.firebase.analytics`).
2. Open **Raccoons → Integrations**, find *Firebase Analytics*, tick **Apply to all platforms**
   and click **Enable**. This adds the `RACCOONS_FIREBASE_ANALYTICS` scripting define. Without
   it `FirebaseAnalyticsService.LogEvent` compiles to a no-op and its constructor logs an error —
   check the define for **both Android and iOS**.
3. Either initialize yourself (resolve Firebase dependencies once, then `Initialize`), or drop a
   **`FirebaseAnalyticsInitializer`** component into your boot scene. The initializer:
   - calls `HoldEvents()` in `Awake` so nothing is lost during async init;
   - runs `FirebaseApp.CheckAndFixDependenciesAsync()`;
   - on success, `Initialize(new FilteredAnalyticsService(new FirebaseAnalyticsService()))`
     (untick *Filter Noisy Events* to send everything);
   - on failure, logs an error;
   - always `ReleaseEvents()`, so events never pile up in memory.
   If your game resolves Firebase dependencies elsewhere (Remote Config, Crashlytics), do it once
   and initialize yourself instead.

`FirebaseAnalyticsService` maps values to `Firebase.Analytics.Parameter`: `string`→string,
`int`/`long`→long, `float`/`double`→double, `bool`→`1`/`0`, `null`→empty string, anything
else→`ToString()`.

## Ad revenue — `RaccoonsAnalytics.AdRevenue`

`ad_impression` is the **only** source of ad revenue for the live-ops dashboard. Safe to call
from any thread.

| Method | Event | Notes |
|--------|-------|-------|
| `AdImpression(adFormat, revenue, placement?, adSource?, adUnitName?, adPlatform = "AppLovin")` | `ad_impression` | `value` (USD, negative → 0), `ad_format` uppercase (`BANNER`, `LEADER`, `MREC`, `INTER`, `REWARDED`), `placement`, `currency=USD`. Carries only `debug`, `store`, `build_version`, `user_level` — no balances or cohorts |
| `AdLoaded(adFormat, revenue, adSource?, adUnitName?, adPlatform)` | `initial_ecpm` | first interstitial of the launch: `value` = revenue × 1000; falls back to the first INTER impression |
| `FlushAdImpressionBatches()` | `ad_impression` | sends open banner batches; called automatically on pause/quit |
| `BannerBatchSeconds` (property, default 0) | — | >0 sums BANNER/LEADER impressions into one event with `impressions` / `impressions_with_value`. **Leave at 0** until rg-liveops reads `impressions` |
| `SessionInitialEcpm` (property) | — | the captured initial eCPM, for adaptive difficulty etc. |

**AppLovin MAX:** `MaxAnalyticsBridge.Attach()` subscribes `OnAdRevenuePaidEvent` of
Interstitial, Rewarded, Banner and MRec plus `Interstitial.OnAdLoadedEvent`. The bridge lives in
the optional assembly `Raccoons.Core.AppLovinMax`, compiled only when the MAX UPM package
`com.applovin.mediation.ads` is installed (with MAX imported as a .unitypackage add the define
`RACCOONS_MAX_AVAILABLE`). It is not attached automatically, so a game that already logs
`ad_impression` itself does not double-count. Pass placements to MAX
(`MaxSdk.ShowInterstitial(unitId, "placement_level_complete_continue")`); MAX copies them into
`AdInfo.Placement`.

**Never also send `*_revenue_paid`** next to `ad_impression` — the dashboard would count the
money twice. `FilteredAnalyticsService` drops them by default.

## Live-ops events — `RaccoonsAnalytics.LiveOps`

| Method | Event | Parameters |
|--------|-------|-----------|
| `CoinsDelta(int delta, string source)` | `coins_delta` | `direction` (`earned` / `spent`), `delta` (+earned / −spent), `source`. One per transaction |
| `ReviveUsed(int level, string source)` | `revive_used` | `level`, `source`. Every revive; never also as `PowerUpUsed` |
| `TutorialStep(track, step, stepCount, finished, variant = "none")` | `open_tutorial_step_<N>_start` / `_finish` | `tutorial`, `step_count`, `variant`. Steps from 1; `stepCount` = number of the last step |
| `Custom(string eventName, Dictionary<string, object> parameters = null)` | any | goes through the normal pipeline; prefer a dedicated method when one fits |

## Event budget

The GA4 → BigQuery daily export (what the live-ops dashboard reads) is limited to 1M events per
day on a standard property; an export consistently over it gets paused and missed days are not
backfilled. Arrow Cube hit it with 512 events per user per day; the dashboard read 38% of them.

`FilteredAnalyticsService` wraps any service and drops `*_load`, `*_load_fail` (ad loading
diagnostics, ~30% of Arrow Cube's volume) and `*_revenue_paid`. Add more names in the
constructor or at runtime with `Block(name)` / `Unblock(name)` (e.g. from Remote Config).
Budget numbers and what not to send: `Documentation~/Analytics/liveops-analytics-setup.md`.

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
`RealCurrency(type, amount)`. `ItemCollected`, `Achievement`, `GiftSent` and `GiftReceived`
need a non-null `Reward`.

## Event catalogue

Most methods take an optional `additionalData` dictionary merged into that event only.
`EventArgs` overloads exist for the Mission methods, ad `Load` / `LoadFail` / `RevenuePaid` /
`Collect`, and `BannerLoad` / `BannerLoadFail`.

> **What the live-ops dashboard needs** (details and verification in the setup doc):
>
> - **Levels / missions** — `mission_started` (every attempt), `mission_completed`,
>   `mission_failed` (at the moment of losing, before any revive offer); `mission_type`,
>   int `mission_id`, `mission_attempt` from 1. Call `SetPlayerLevel` before each level.
> - **Ad revenue** — `ad_impression` via `AdImpression` / `MaxAnalyticsBridge`, plus
>   `initial_ecpm`.
> - **Rewarded funnel** — `reward_video_show`, `reward_video_collect`.
> - **Boosters** — `power_up_used` for every booster; revives as `revive_used`.
> - **Economy / tutorial** — `coins_delta`, `open_tutorial_step_*` if the game has them.
> - **IAP** — do **not** log `in_app_purchase` yourself: the name is reserved by Firebase,
>   which collects it automatically on Google Play.
>
> Everything else in the catalogue is optional and costs event budget.

### Missions — `RaccoonsAnalytics.Mission`
Common args: `missionType`, `missionName`, `missionID`, `missionAttempt?`, `isGamePlay?`.

| Method | Event | |
|--------|-------|---|
| `MissionStarted(...)` | `mission_started` | **required** |
| `MissionCompleted(..., Reward reward = null)` | `mission_completed` | **required** |
| `MissionFailed(..., string failReason = null)` | `mission_failed` | **required** |
| `MissionAbandoned(...)` | `mission_abandoned` | optional |
| `MissionStep(..., Reward reward, string stepName)` | `mission_step` | optional |

### Ads funnel — `RaccoonsAnalytics.Ads`
Common args: `placement`, `network` (→ `ad_provider`), `level?` (→ `level_num`; the dashboard
reads `user_level` from `SetPlayerLevel`, not `level_num`).

**required:** `reward_video_show`, `reward_video_collect`. Revenue comes from `ad_impression`
(above), not from this funnel. The rest is optional diagnostics, 3–6 events per user per day
each.

- **Interstitial:** `interstitial_load`\*, `interstitial_load_fail`\*, `interstitial_try_show`,
  `interstitial_show_requested`, `interstitial_show`, `interstitial_show_fail`,
  `interstitial_end`, `interstitial_click`, `interstitial_revenue_paid`\*.
- **Rewarded video:** `reward_video_load`\*, `reward_video_load_fail`\*, `reward_video_try_show`,
  `reward_video_show_requested`, **`reward_video_show` (required)**, `reward_video_show_fail`,
  `reward_video_end`, `reward_video_click`, **`reward_video_collect` (required)**,
  `reward_video_opportunity`, `reward_video_revenue_paid`\*.
- **Banner:** `banner_load`\*, `banner_load_fail`\*, `banner_show_requested`, `banner_show`,
  `banner_show_fail`, `banner_hide`, `banner_revenue_paid`\*.

\* dropped by `FilteredAnalyticsService` by default.

Failures take an `AdErrorType` (`Undefined`, `NoFill`, `NetworkError`, `AdExpired`,
`InvalidRequest`, `InternalError`) → `error_reason`. Revenue methods add a `revenue` (double).

### Monetization — `RaccoonsAnalytics.Monetization`

| Method | Event | |
|--------|-------|---|
| `Economy(Transaction, placement = "General")` / `(name, spent, received, …)` | `economy` | optional |
| `InAppPurchase(Transaction, placement, ReceiptStatus, bool isTestPurchase)` | `in_app_purchase` | avoid with Firebase: reserved name, hand-sent events may be dropped |

`ReceiptStatus`: `NoValidation`, `Success`, `Failure`.

### In-game — `RaccoonsAnalytics.InGame`

**`power_up_used` (required)** — log it for every booster/power-up the player activates
(`PowerUpUsed(missionID, missionType, missionAttempt, powerUpName, missionName)`; note
`missionID` is a string here).

Optional: `item_collected`, `shop_entered`, `achievement`, `ui_interaction`,
`feature_unlocked`, `level_up`, `skill_upgraded`, `skill_used`, `character_created`,
`character_updated`, `character_deleted`, `options`, `gift_sent`, `gift_received`,
`item_actioned`, `product_viewed`, `notification_opened`, `notification_scheduled`,
`notification_cancelled`, `hand_action`.

## Sample

`Samples/Analytics/LevelFlowSample/` is a demo: an IMGUI panel (`AnalyticsLevelFlowSample.cs`)
that fires realistic level flows (start → power-up → complete, soft-fail → ad-revive →
complete, checkpoints, abandon, …) through `RaccoonsAnalytics`, plus a live event log driven by
`OnLogEvent`. Open the `AnalyticsLevelFlow` scene and press Play. The sample calls `Initialize`
itself — do not ship that scene in a game build.
