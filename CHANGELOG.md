# Changelog

All notable changes to this package are documented here.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [0.1.0] - 2026-10-26

Everything a game needs to show up in the live-ops dashboard now lives in the package.
**A live game upgrades only together with live-ops** — see *Upgrading from 0.0.x* below.

### Added
- `RaccoonsAnalytics.Custom(eventName, parameters)` — same signature as the copy embedded in Arrow Cube.
- Ad revenue: `AdImpression` → `ad_impression`, `AdLoaded` → `initial_ecpm`, optional banner batching
  (`BannerBatchSeconds`, off by default), `FlushAdImpressionBatches`, `SessionInitialEcpm`. Thread-safe.
- `CoinsDelta`, `ReviveUsed`, `TutorialStep` with the live-ops dashboard's event names and types.
- `FilteredAnalyticsService` — drops `*_load`, `*_load_fail`, `*_revenue_paid`; `Block` / `Unblock` at runtime.
- `Raccoons.Core.AppLovinMax` assembly with `MaxAnalyticsBridge` (compiled only with `com.applovin.mediation.ads`).
- Development builds warn on invalid, reserved or numbered event names and on more than 25 parameters.
- `Documentation~/Analytics/` — live-ops setup guide and a setup file for AI assistants.

### Changed
- `Initialize` sets `build_version` to `Application.version` when the game has not set it.
- `Initialize` caches the install mode and creates a hidden `[RaccoonsAnalytics]` GameObject
  (`DontDestroyOnLoad`) that flushes banner batches on pause/quit.
- `FirebaseAnalyticsInitializer` always releases held events, also when Firebase is unavailable, and can wrap
  Firebase in `FilteredAnalyticsService` (*Filter Noisy Events*, **off by default**).
- `FirebaseAnalyticsService` logs an error when `RACCOONS_FIREBASE_ANALYTICS` is not defined.
- Readme: `ad_impression` is the revenue source; `*_revenue_paid` must not be sent next to it.

### Upgrading from 0.0.x
- **Filter is opt-in.** `FirebaseAnalyticsInitializer` keeps sending everything until *Filter Noisy Events* is
  ticked; games that call `Initialize` themselves are unaffected unless they wrap their service.
- **`build_version` default.** A game that never called `SetBuildVersion` starts sending
  `build_version = Application.version` on the day of the release. From that day the dashboard's
  invalid-traffic filter applies to its level funnel — write it into the game's history so a step in the
  numbers is not misread.
- **Hidden GameObject.** `Initialize` (in Play mode) creates `[RaccoonsAnalytics]` with `DontDestroyOnLoad`.
- **Live games update the package only through live-ops**: they check what the change does to the game's
  dashboards and daily reports first.
