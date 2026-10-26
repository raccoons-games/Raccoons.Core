# RACCOONS_ANALYTICS_AI_SETUP.md

Instructions for an AI coding assistant working inside a Unity game repo.

**Goal:** wire analytics through `Raccoons.Core` (Runtime/Analytics) + Firebase so the game appears in the
Raccoons live-ops dashboard (rg-liveops) and its daily TaskFlow reports with **no dashboard-side work besides
registration**, while sending **fewer than 1,000,000 events per day** (the GA4 standard daily BigQuery export
limit. Google documents that an export consistently over it gets paused and missed days are not backfilled;
Arrow Cube's daily tables stopped twice, each time after about a week above 1M).

Out of scope: AppsFlyer (ad revenue for ROAS, postback events) is a separate integration. Do not treat
"analytics done" here as covering it.

Then **verify** everything and report back in the format at the end.

Everything the game needs is in the SDK (version 0.1.0+): typed methods for every dashboard event, ad revenue
(`AdImpression` / `MaxAnalyticsBridge`), and `FilteredAnalyticsService` against spam. No per-game helper files.
The human-readable version of this guide is `Documentation~/Analytics/liveops-analytics-setup.md` in the package.

Reference game: Arrow Cube (best-instrumented, but also the noisiest — the "do not copy" list matters).
Event contract source of truth: `rg-liveops/docs/analytics-event-contract.md`. If this file and the contract
disagree, the contract wins — tell the human.

---

## 0. Hard rules (read before touching code)

1. **Event names are a contract.** Use exactly the names/params below. A renamed event = a silently empty
   dashboard page.
2. **Static event names only.** Never put a level number, variant, placement or any value into an event name.
   The single exception: tutorial step number in `open_tutorial_step_<N>_start|finish`. GA4 caps distinct
   event names at ~500.
3. **Never call `RaccoonsAnalytics.InterstitialRevenuePaid / RewardVideoRevenuePaid / BannerRevenuePaid`.**
   Revenue lives only on `ad_impression.value`; the revenue_paid events double-count it.
4. **`ad_format` = MAX format code as-is, uppercase** (`BANNER`, `LEADER`, `MREC`, `INTER`, `REWARDED`).
   Never map or lowercase it.
5. **Do not touch AppsFlyer events** (they drive UA postbacks and campaign optimisation). Only Firebase/GA4
   events are in scope. If an AppsFlyer event looks redundant, list it in the report; do not remove it.
   In Arrow Cube one game method fans out to Firebase, AppsFlyer, GameAnalytics and Raccoons at once — so when
   trimming, cut only the Firebase branch, never the shared call site. **If the game is live with paid UA,
   remove or rename nothing without the human's explicit go-ahead**, and ask which GA4 events are imported
   into Google Ads as conversions.
6. **Do not add events the dashboard does not read** unless the human names a concrete question they answer.
   Every event costs budget (section 5).
7. **No per-frame / per-tap / per-move / per-refresh events.** Anything that can fire more than ~10 times per
   session is a counter in a parameter or a batch, not an event.
8. Ask the human before: changing ad refresh rates, removing an existing event that someone may use,
   enabling banner batching, or anything touching consent/ATT.
9. Do not commit secrets, do not edit `google-services.json` / `GoogleService-Info.plist` contents.

---

## 1. Inspect first (no edits yet)

Produce an inventory and show it to the human before changing anything.

1. Unity version; `Packages/manifest.json`: is `com.raccoons.core` present? pinned to a commit? Is there an
   embedded copy under `Packages/com.raccoons.core/` (it overrides the manifest URL)?
2. Zenject/Extenject present as a UPM package (`com.svermeulen.extenject`, `com.mathijsbakker.extenject`,
   `com.modesttree.zenject`) or as a .unitypackage?
3. Scripting define symbols for **both** Android and iOS in `ProjectSettings/ProjectSettings.asset`
   (`scriptingDefineSymbols`): `RACCOONS_FIREBASE_ANALYTICS`, `RACCOONS_DEPS_AVAILABLE`.
4. Firebase Unity SDK modules and version (`Assets/Firebase/`, `*Dependencies.xml`): Analytics, Crashlytics.
   AppLovin MAX plugin version. AppsFlyer present?
5. Every analytics call site:
   `grep -rnE "RaccoonsAnalytics\.|FirebaseAnalytics\.LogEvent|AppsFlyer\.(sendEvent|logAdRevenue)|GameAnalytics\." Assets`
6. Game hooks: level start / win / lose / quit / retry, revive, boosters, coin balance changes, tutorial steps,
   every `MaxSdk.Show*` call, every `MaxSdkCallbacks.*` subscription, IAP flow.
7. Is `FirebaseAnalyticsInitializer` (from the SDK) used in any scene/prefab? Find its script GUID in
   `Packages/**/FirebaseAnalyticsInitializer.cs.meta` (or the package cache) and grep `*.unity`/`*.prefab` for it.

Inventory table to show: `event_name | file:line | trigger | est. fires per session | in contract? | action
(keep / add / remove / aggregate / ask)`.

Also ask the human (if not already given): planned DAU, does the game have revive / tutorial / IAP / soft
currency, list of rewarded & interstitial placements, GCP project id and BigQuery dataset
(`analytics_<GA4 property id>`).

---

## 2. Install

1. Zenject/Extenject as a UPM package (so `RACCOONS_DEPS_AVAILABLE` is set automatically). If the project uses
   a .unitypackage Zenject, add `RACCOONS_DEPS_AVAILABLE` manually to Android and iOS defines.
2. `Packages/manifest.json` — pin the SDK to a commit (no tag contains Analytics yet):
   ```json
   "com.raccoons.core": "https://github.com/raccoons-games/Raccoons.Core.git#0d54dd7f9c04d3698d75968c78fcfac5597e9cfa"
   ```
   This is the first commit with the live-ops API (package version 0.1.0). Until branch `feat/liveops-analytics`
   is merged into main, ask the human for the current tag/commit and use that instead. Verify after resolve:
   `Library/PackageCache/com.raccoons.core*/package.json` → version ≥ 0.1.0, and
   `Runtime/Analytics/RaccoonsAnalytics.AdRevenue.cs` exists.
   If an embedded copy exists in `Packages/com.raccoons.core/`, do not delete it silently — report it to the human
   (it overrides the manifest URL; its `Custom` has the same signature as 0.1.0, so switching is code-compatible).
3. Firebase Unity SDK: Analytics (required), Crashlytics (strongly recommended — crash lines in the dashboard).
   AppLovin MAX: as the UPM package `com.applovin.mediation.ads` the SDK's `Raccoons.Core.AppLovinMax` assembly
   (with `MaxAnalyticsBridge`) compiles automatically; with MAX imported as a .unitypackage add the scripting
   define `RACCOONS_MAX_AVAILABLE` to Android and iOS.
4. Add `RACCOONS_FIREBASE_ANALYTICS` to **both Android and iOS** define groups. Without it
   `FirebaseAnalyticsService.LogEvent` compiles to an empty body: logs say "SENT", Firebase receives nothing
   (since 0.1.0 the service constructor at least logs an error).
   The Raccoons → Integrations window only sets the current platform unless "Apply to all platforms" is ticked.
5. Disable automatic screen reporting (on iOS it logs every ad / App Store view controller as a screen —
   Arrow Cube: 52 `screen_view` per user per day, mostly ads; the dashboard does not read it):
   - Android, `Assets/Plugins/Android/AndroidManifest.xml` inside `<application>`:
     ```xml
     <meta-data android:name="google_analytics_automatic_screen_reporting_enabled" android:value="false" />
     ```
   - iOS, an Editor script (e.g. `Assets/Editor/FirebasePlistPostProcess.cs`):
     ```csharp
     #if UNITY_IOS
     using System.IO;
     using UnityEditor;
     using UnityEditor.Callbacks;
     using UnityEditor.iOS.Xcode;

     public static class FirebasePlistPostProcess
     {
         [PostProcessBuild]
         public static void OnPostprocessBuild(BuildTarget target, string path)
         {
             if (target != BuildTarget.iOS) return;
             var plistPath = Path.Combine(path, "Info.plist");
             var plist = new PlistDocument();
             plist.ReadFromFile(plistPath);
             plist.root.SetBoolean("FirebaseAutomaticScreenReportingEnabled", false);
             plist.WriteToFile(plistPath);
         }
     }
     #endif
     ```
     If the project already has an iOS PostProcessBuild that edits Info.plist, add the line there instead.

---

## 3. Initialise

If nothing else in the game initialises Firebase, the SDK's `FirebaseAnalyticsInitializer` component (boot scene)
is enough: since 0.1.0 it always releases held events and wraps Firebase in `FilteredAnalyticsService`. If Remote
Config / Crashlytics also wait for Firebase, initialise in the game's boot flow instead, in this order:

```csharp
using Raccoons.Analytics;
using Raccoons.Analytics.Firebase;
using Raccoons.Analytics.AppLovinMax;

RaccoonsAnalytics.HoldEvents();                               // queue anything logged before Firebase is ready
try
{
    // Once per process, shared with Remote Config / Crashlytics. Reuse the game's existing gate if it has one.
    var status = await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();
    if (status == Firebase.DependencyStatus.Available)
        RaccoonsAnalytics.Initialize(new FilteredAnalyticsService(new FirebaseAnalyticsService()));
}
finally
{
    RaccoonsAnalytics.ReleaseEvents();
}

// After MaxSdk.InitializeSdk():
MaxAnalyticsBridge.Attach();
```

- `Initialize` exactly once (each call emits another `game_started`).
- `Initialize` sets `build_version` to `Application.version`. Do not override it with `SetBuildVersion(...)` using
  a versionCode or hard-coded string: the dashboard marks every user as invalid traffic and drops them from the
  level funnel.
- `Application.version` must look like `1.2.3` (the daily report ignores other formats).
- The SDK stamps `debug` ("true" in Editor/DEVELOPMENT_BUILD), `store` (installMode == Store) and
  `build_version` on every event, including `ad_impression`.
- Banner batches (if ever enabled) are flushed by the SDK itself on pause/quit.
- **One revenue path only:** if the game already logs `ad_impression` itself (own MAX callbacks →
  `FirebaseAnalytics.LogEvent("ad_impression", …)`), either remove that code and `Attach()` the bridge, or keep it
  and do not attach — never both. Report which one you chose.

---

## 4. SDK API used by this guide

Do not write wrapper files for these; call the SDK. Do not change event or parameter names.

| Call | Event | Notes |
|---|---|---|
| `MaxAnalyticsBridge.Attach()` | `ad_impression`, `initial_ecpm` | MAX only; subscribes revenue of Interstitial/Rewarded/Banner/MRec + interstitial load |
| `RaccoonsAnalytics.AdImpression(format, revenueUsd, placement, adSource, adUnitName, adPlatform)` | `ad_impression` | other mediations; any thread; negative revenue → 0 |
| `RaccoonsAnalytics.AdLoaded(format, revenueUsd, adSource, adUnitName, adPlatform)` | `initial_ecpm` | other mediations; first INTER of the launch |
| `RaccoonsAnalytics.CoinsDelta(delta, source)` | `coins_delta` | |
| `RaccoonsAnalytics.ReviveUsed(level, source)` | `revive_used` | |
| `RaccoonsAnalytics.TutorialStep(track, step, stepCount, finished, variant)` | `open_tutorial_step_<N>_start/_finish` | |
| `RaccoonsAnalytics.Custom(name, params)` | any | events without a dedicated method; dev builds warn on bad names |
| `new FilteredAnalyticsService(inner, extraBlocked)`, `.Block(name)` / `.Unblock(name)` | — | drops `*_load`, `*_load_fail`, `*_revenue_paid` by default |
| `RaccoonsAnalytics.BannerBatchSeconds` | — | default 0; do not change (section 6) |
| `RaccoonsAnalytics.SessionInitialEcpm` | — | captured initial eCPM (e.g. for adaptive difficulty) |

---

## 5. Wire the contract

### 5.1 Events the dashboard reads — implement all that apply

| Event | Call | Required params (type) | When | Lights up |
|---|---|---|---|---|
| `mission_started` | `RaccoonsAnalytics.MissionStarted("level", $"level_{n}", n, attempt, isGamePlay: true)` | `mission_type` s, `mission_id` **int**, `mission_name` s, `mission_attempt` int | every attempt incl. retries | level funnel, Levels, retention by level, Version health, D0 trend, daily report |
| `mission_completed` | `RaccoonsAnalytics.MissionCompleted("level", $"level_{n}", n, attempt, isGamePlay: true)` | same | real win only | same |
| `mission_failed` | `RaccoonsAnalytics.MissionFailed("level", $"level_{n}", n, attempt, failReason: "...", isGamePlay: true)` | same + `fail_reason` s | at the moment of losing, **before** any revive offer | same |
| `user_level` (param) | `RaccoonsAnalytics.SetPlayerLevel(n)` before `MissionStarted` | int | — | attributes RV/booster events to a level |
| `ad_impression` | `MaxAnalyticsBridge.Attach()` once (MAX) or `RaccoonsAnalytics.AdImpression(...)` from the mediation's revenue callback | `value` **double** USD, `ad_format` s UPPERCASE, `placement` s, `currency`=USD | every paid impression | Revenue, Ad detail, eCPM, D0 trend, ARPU per level |
| `initial_ecpm` | automatic with the bridge, or `RaccoonsAnalytics.AdLoaded(...)` from the interstitial load callback | `value` double (eCPM) | once per launch | eCPM tiers |
| `reward_video_show` | `RaccoonsAnalytics.RewardVideoShow(placement, networkName, n)` | `placement` s, `user_level` int (from `SetPlayerLevel` only) | rewarded shown | RV funnel, RV adoption |
| `reward_video_collect` | `RaccoonsAnalytics.RewardVideoCollect(placement, reward, n)` | `placement` s, `user_level` int (from `SetPlayerLevel` only) | reward granted | RV funnel |
| `power_up_used` | `RaccoonsAnalytics.PowerUpUsed(n.ToString(), "level", attempt, boosterName, $"level_{n}")` | `mission_id`, `power_up_name` s | one per booster use, all booster types except revive | boosters per level |
| `revive_used` | `RaccoonsAnalytics.ReviveUsed(n, source)` | `level` int | every revive (if the game has revives); never also as `power_up_used` | revive rate |
| `coins_delta` | `RaccoonsAnalytics.CoinsDelta(delta, source)` | `direction` s, `delta` int (+earned / −spent), `source` s | one per transaction | coin economy in D0 trend |
| `open_tutorial_step_<N>_start` / `_finish` | `RaccoonsAnalytics.TutorialStep(track, n, stepCount, finished)` | `tutorial` s, `step_count` int, `variant` s | each step (if tutorial) | tutorial funnel |

Placements: pass them to MAX — `MaxSdk.ShowInterstitial(unitId, "placement_<screen>_<action>")`,
`MaxSdk.ShowRewardedAd(unitId, "placement_...")`. Always `placement_` prefix, snake_case, stable names
(Arrow Cube examples: `placement_level_complete_continue`, `placement_out_of_moves_revive`,
`placement_level_complete_double`). MAX copies it into `AdInfo.Placement`, the bridge puts it on
`ad_impression`. Use the same string in `RewardVideoShow/Collect`.

`mission_attempt`: 1-based, +1 per retry of the same level, **persisted** (PlayerPrefs per level) so a relaunch
does not reset it.

**Fail / revive order.** `mission_failed` fires when the player loses, before the revive offer. A revive is
`revive_used` and the attempt continues: no new `mission_started`, same `mission_attempt`. The attempt ends with
`mission_completed` or another `mission_failed`. Logging the fail only after a declined revive makes the
dashboard's revive rate inflated, up to above 100%, and understates soft fails.

**`user_level`.** RV events get the level from `user_level`, which only `RaccoonsAnalytics.SetPlayerLevel` sets.
The `level` argument of `RewardVideoShow/Collect` lands in `level_num`, which the dashboard does not read.

**Tutorial.** Steps are numbered from 1; `stepCount` is the number of the last step (the dashboard counts a
player as having completed the tutorial on `step_<stepCount>_finish`).

**Optional:** `dd` (string) on `mission_*` if the game has adaptive difficulty — feeds the DDA pages.

Other `mission_type` families (challenges, events) get their own type; never reuse `"level"` for them. Any value
other than `"level"` for the main levels must be registered by live-ops, or the level funnel stays empty.

### 5.2 Firebase sends these by itself — leave them alone

`user_engagement` (play time), `session_start`, `first_open`, `app_update`, `app_remove`, `app_exception`
(Crashlytics). IAP: do **not** log `in_app_purchase` manually (reserved name). Android auto-collects it. The
dashboard reads the amount from the row field `event_value_in_usd`, which Firebase fills itself. For iOS with
StoreKit 2, a comment in Arrow Cube's code says auto-collection is missing and `LogAppleTransactionAsync`
(Firebase Unity 13.12+) is needed — unverified by us. If the game has IAP on iOS, flag it to the human and
include the IAP check from 7.3.

### 5.3 Do not send (the "do not copy from Arrow Cube" list)

| Pattern | Why |
|---|---|
| `*_load`, `*_load_fail` (interstitial, rewarded, banner) | 30% of Arrow Cube volume, read by nothing. `FilteredAnalyticsService` drops the SDK ones; remove direct calls. If load diagnostics are needed, sample ~5% of users by a hash of the install id |
| `*_revenue_paid` | double-counts revenue |
| per-tap / per-move / per-mistap events (`input_mistap`, `move_batch`, `level_first_action`) | 30+ per user per day; put counts in a `mission_completed` param instead |
| "element shown" events that repeat (`hint_side_shown`) | at most once per level |
| `level_<N>_start/complete/fail` style names | 926 distinct names/day in Arrow Cube; GA4 caps an app at ~500 names, beyond it new names may be missing from reports (an AC audit saw top levels disappear) |
| `hint_used` alongside `power_up_used` | dashboard sums both → boosters double-counted |
| automatic `screen_view` | see 2.5 |
| SDK ad-funnel extras (`*_show_requested`, `*_click`, `*_end`, `reward_video_opportunity`, `banner_show/hide`) | not read by the dashboard; 3–6 events per user each. Only with a named consumer |

---

## 6. Budget check (must pass before you finish)

Budget per user per day: `800,000 / (planned_DAU × 1.5)`.

| DAU | max events per user/day |
|---:|---:|
| 1,000 | 533 |
| 3,000 | 178 |
| 5,000 | 107 |
| 10,000 | 53 |

What the contract itself costs, measured on Arrow Cube 2026-10-24 (both platforms, per user per day):
`user_engagement` 31.4 (iOS 40.2, Android 7.5), `mission_*` 17.0, inter+rewarded `ad_impression` 5.9, RV
show+collect 5.1, `power_up_used`+`revive_used` 3.2, `game_started` 2.9 (the SDK sends one per `Initialize`),
`initial_ecpm` 2.7, `session_start` 1.9, `coins_delta` 0.9 (Android only in AC; 3.4 there), `first_open` +
`app_exception` 0.25. **Total ≈71 without banners.**

- iOS `user_engagement` is inflated by the same automatic screen tracking as `screen_view` (70% of it is on ad /
  StoreKit view controllers). Whether it drops after 2.5 is a hypothesis — Android's 7.5 is not proof (Android
  AC still has tracking on, it just has few screens: 1.1 `screen_view` per user). Check it on the first full day.
- **Banners:** one `ad_impression` per 15 s refresh adds ≈123 per user (Arrow Cube: 95% of impressions, 6% of
  ad revenue) → ≈195 total, which fits the budget only up to ~2,700 DAU (hard 1M line at ~5,100 DAU). Banner
  batches of 300 s add ≈10 instead → ≈82 total, ~6,500 DAU within budget, ~12,000 at the hard line. So any
  game with a 15 s banner planning > ~2,700 DAU needs batching — and batching needs the dashboard change first
  (a live-ops task; raise it with the human before UA starts).

Estimate events/user/day for **every** event in your inventory (fires per session × ~2 sessions/day; heavy
players do 4+). If the total exceeds the budget:

1. remove events from 5.3 first;
2. turn repeated events into counters in params;
3. banners: propose `RaccoonsAnalytics.BannerBatchSeconds = 300` to the human — **do not enable it yourself**. Revenue
   stays correct, but until rg-liveops reads the `impressions` param it breaks: impression counts, banner eCPM
   (×10–12) and fill in the dashboard; per-format eCPM and alerts in the daily report (on the switch day too);
   the batch loses `ad_source`; and if `ad_impression` is imported into Google Ads as a conversion, the
   conversion count drops sharply while the value sum stays. AppsFlyer is not affected;
4. last resort (account owner, not code): exclude events from the BigQuery export in GA4 Admin → BigQuery Links.

---

## 7. Verify

### 7.1 Static checks (run them, paste results)

| Check | Command / method | Pass |
|---|---|---|
| SDK version | `package.json` of the resolved `com.raccoons.core` | ≥ 0.1.0 |
| no SDK revenue_paid calls | first grep below the table | 0 hits |
| no dynamic event names | the other greps below the table + review of any event name built from variables (incl. `RaccoonsAnalytics.Custom($"…")`) | 0 hits in game code (tutorial step names are built inside the SDK) |
| defines on both platforms | inspect `scriptingDefineSymbols` in `ProjectSettings.asset` | `RACCOONS_FIREBASE_ANALYTICS` (+ `RACCOONS_DEPS_AVAILABLE` if needed) for Android **and** iOS |
| SDK pinned | `Packages/manifest.json` | `#<commit>` present |
| screen reporting off | AndroidManifest meta-data + iOS PostProcessBuild present | both |
| no `FirebaseAnalyticsInitializer` in scenes | GUID grep from step 1.7 | 0 hits |
| no events in loops | review each call site: not in `Update/LateUpdate/FixedUpdate`, not per frame/tap | none |
| Firebase limits | event names ≤ 40 chars snake_case; ≤ 25 params per event; string values ≤ 100 chars | all |
| one revenue path | revenue grep below: either `MaxAnalyticsBridge.Attach` or the game's own `ad_impression` / `AdImpression` calls, not both | yes |
| compiles | Unity batchmode compile or the project's CI for Android and iOS targets | 0 errors |

```bash
# SDK revenue_paid calls (must be 0)
grep -rnE 'RaccoonsAnalytics\.(Interstitial|RewardVideo|Banner)RevenuePaid\(' Assets
# dynamic event names
grep -rnE 'LogEvent\(\s*\$"' Assets              # interpolated names
grep -rnE 'LogEvent\(\s*"[a-z_0-9]*"\s*\+' Assets   # concatenated names
grep -rnE 'Custom\(\s*\$"' Assets                  # RaccoonsAnalytics.Custom with interpolation
# revenue paths (expect exactly one kind)
grep -rnE 'MaxAnalyticsBridge\.Attach|RaccoonsAnalytics\.AdImpression\(|"ad_impression"' Assets
```

### 7.2 On device (the human runs it; give them this script)

The Unity Editor does not send to Firebase. Use a device and DebugView:
- Android: `adb shell setprop debug.firebase.analytics.app <bundle_id>` (off: `adb shell setprop debug.firebase.analytics.app .none.`)
- iOS: Xcode scheme → Arguments Passed On Launch → `-FIRDebugEnabled`

Firebase Console → Analytics → DebugView. Play: start a level, win it, start the next, lose it, retry, use a
booster, revive (if any), watch one interstitial and one rewarded, let a banner run 1 minute, earn/spend coins,
go through the tutorial (fresh install).

Expect: `mission_started/completed/failed` with int `mission_id` and growing `mission_attempt`;
`ad_impression` per format with `value`, uppercase `ad_format`, `placement_*` on INTER/REWARDED;
one `initial_ecpm`; `reward_video_show` + `reward_video_collect`; `power_up_used`; `coins_delta`;
`open_tutorial_step_*`. Must **not** appear: `*_load`, `*_load_fail`, `*_revenue_paid`, `screen_view`.

The Raccoons "SENT" log fires **before** delivery (and even for events the guard drops) — it proves nothing.

### 7.3 BigQuery, on the first full day after release

Replace `PROJECT.DATASET` and `YYYYMMDD` (a completed day). Each query reads only the columns it needs.

Daily table sizes (metadata only, free):
```sql
SELECT table_id, row_count
FROM `PROJECT.DATASET.__TABLES__`
WHERE table_id LIKE 'events_2%'
ORDER BY table_id DESC
LIMIT 14;
```
Pass: a table for every day, each < 800,000 rows. A missing day after days > 1M = export paused.

Volume and events per user:
```sql
SELECT platform,
       COUNT(*) AS events,
       COUNT(DISTINCT user_pseudo_id) AS dau,
       ROUND(COUNT(*) / COUNT(DISTINCT user_pseudo_id), 1) AS events_per_user
FROM `PROJECT.DATASET.events_*`
WHERE _TABLE_SUFFIX = 'YYYYMMDD'
GROUP BY ROLLUP(platform);
```
Pass: events_per_user within the section 6 budget.

Top events and distinct names:
```sql
WITH d AS (
  SELECT event_name, user_pseudo_id
  FROM `PROJECT.DATASET.events_*`
  WHERE _TABLE_SUFFIX = 'YYYYMMDD'
),
dau AS (SELECT COUNT(DISTINCT user_pseudo_id) AS n FROM d)
SELECT event_name,
       COUNT(*) AS events,
       ROUND(100 * COUNT(*) / SUM(COUNT(*)) OVER (), 2) AS pct,
       ROUND(COUNT(*) / ANY_VALUE(dau.n), 1) AS per_user,
       COUNT(*) OVER () AS distinct_names
FROM d CROSS JOIN dau
GROUP BY event_name
ORDER BY events DESC;
```
Pass: `distinct_names` < 100; nothing from 5.3 in the list; no single event outside 5.1 **and 5.2** above ~5%
of volume (automatic events such as `user_engagement` legitimately take 10%+ in a clean game — leave them alone).

Parameter types:
```sql
SELECT event_name, ep.key,
       COUNTIF(ep.value.int_value IS NOT NULL)    AS int_n,
       COUNTIF(ep.value.double_value IS NOT NULL) AS double_n,
       COUNTIF(ep.value.string_value IS NOT NULL) AS string_n,
       COUNT(*) AS n
FROM `PROJECT.DATASET.events_*`, UNNEST(event_params) AS ep
WHERE _TABLE_SUFFIX = 'YYYYMMDD'
  AND event_name IN ('mission_started','mission_completed','mission_failed','ad_impression','initial_ecpm',
                     'reward_video_show','reward_video_collect','power_up_used','revive_used','coins_delta')
  AND ep.key IN ('mission_id','mission_type','mission_attempt','user_level','value','ad_format','placement',
                 'level','delta','direction','debug','store','build_version')
GROUP BY event_name, ep.key
ORDER BY event_name, ep.key;
```
Pass: `mission_id`, `mission_attempt`, `user_level`, `level`, `delta`, `store` → int; `value` → double;
`mission_type`, `ad_format`, `placement`, `direction`, `debug`, `build_version` → string. (`power_up_used`
sends `mission_id` as string — a known, tolerated SDK quirk.)

Ad formats and placements:
```sql
SELECT (SELECT value.string_value FROM UNNEST(event_params) WHERE key = 'ad_format') AS ad_format,
       COUNT(*) AS impressions,
       COUNTIF(STARTS_WITH((SELECT value.string_value FROM UNNEST(event_params) WHERE key = 'placement'), 'placement_')) AS with_placement,
       ROUND(SUM((SELECT value.double_value FROM UNNEST(event_params) WHERE key = 'value')), 2) AS revenue_usd
FROM `PROJECT.DATASET.events_*`
WHERE _TABLE_SUFFIX = 'YYYYMMDD' AND event_name = 'ad_impression'
GROUP BY ad_format
ORDER BY impressions DESC;
```
Pass: only uppercase formats; INTER and REWARDED `with_placement` ≈ `impressions`; revenue ≈ MAX dashboard
for the same day (± a few %).

Build version consistency:
```sql
SELECT app_info.version AS app_version,
       (SELECT value.string_value FROM UNNEST(event_params) WHERE key = 'build_version') AS build_version,
       COUNT(*) AS n
FROM `PROJECT.DATASET.events_*`
WHERE _TABLE_SUFFIX = 'YYYYMMDD' AND event_name = 'mission_started'
GROUP BY 1, 2
HAVING build_version IS NULL OR build_version != app_version;
```
Pass: 0 rows.

Ad revenue the daily report could drop (`store=0` impressions are excluded by its QA filter):
```sql
SELECT platform,
       COUNT(*) AS impressions,
       COUNTIF((SELECT value.int_value FROM UNNEST(event_params) WHERE key = 'store') = 0) AS store0,
       COUNTIF((SELECT value.string_value FROM UNNEST(event_params) WHERE key = 'debug') = 'true') AS debug_true
FROM `PROJECT.DATASET.events_*`
WHERE _TABLE_SUFFIX = 'YYYYMMDD' AND event_name = 'ad_impression'
GROUP BY platform;
```
Pass: `store0` and `debug_true` ≈ 0 on a release build. Otherwise tell live-ops (e.g. Android distribution
outside Google Play).

IAP (only if the game sells anything):
```sql
SELECT platform, event_name,
       COUNT(*) AS purchases,
       COUNTIF(event_value_in_usd > 0) AS with_usd,
       ROUND(SUM(event_value_in_usd), 2) AS usd
FROM `PROJECT.DATASET.events_*`
WHERE _TABLE_SUFFIX = 'YYYYMMDD' AND event_name IN ('in_app_purchase', 'purchase')
GROUP BY platform, event_name;
```
Pass: rows for both platforms, `with_usd` ≈ `purchases`, `usd` ≈ store console revenue for the day.

---

## 8. Hand-off outside the code (tell the human)

- **Firebase/GA4 owner:** BigQuery link with **Daily** export ON (reports read only `events_YYYYMMDD`),
  Streaming optional (paid; fresher data and a safety net during export pauses), **User data** export OFF
  (its tables break the dashboard), GCP billing ON (sandbox tables expire in 60 days). Grant
  `liveops-bq-reader@arrows-cube.iam.gserviceaccount.com` *BigQuery Job User* on the game's GCP project and
  *BigQuery Data Viewer* on the `analytics_<propertyId>` dataset.
- **live-ops:** needs GCP project id, dataset, both bundle ids / store ids and the GA4 property time zone to
  register the game in `lib/projects.ts` and the TaskFlow daily report. The daily report switches on once a
  version has ≥ 25 users per day.

---

## 9. Report back in this format

1. **Inventory** — the table from step 1, with the final action per event.
2. **Changes** — files touched, one line each.
3. **Budget** — estimated events/user/day per event and total vs the section 6 budget for the planned DAU.
4. **Static checks** — the 7.1 table with actual results.
5. **Needs the human** — device run (7.2), BigQuery checks after release (7.3), hand-off (8), and every
   decision you were not allowed to take (banner batching, AppsFlyer events, ad refresh, embedded SDK copy).
6. **Open risks** — anything in the game that did not fit the contract.
