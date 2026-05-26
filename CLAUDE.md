# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Raccoons.Core** is a Unity game framework package (`com.raccoons.core`) distributed via Unity Package Manager (UPM). It requires Unity 2021.1+, **Zenject** (dependency injection), and **Newtonsoft.Json**.

After any script changes, run "dotnet run" to check if there is any errors in code.

## Running Tests

Tests live in `Tests/` and use NUnit with Unity's `[UnityTest]` attribute. Run them via **Window > General > Test Runner** in the Unity Editor (EditMode tests). There is no headless test runner configured.

## Assembly Structure

| Assembly | Location | Purpose |
|----------|----------|---------|
| `Raccoons.Core.asmdef` | `Runtime/` | Core framework (namespace: `Raccoons`) |
| `Raccoons.Core.Editor.asmdef` | `Editor/` | Editor-only tools and inspectors |
| `Raccoons.Tests.asmdef` | `Tests/` | NUnit test suite (Editor-only) |
| `Raccoons.Core.Samples.asmdef` | `Samples/` | Usage examples |

## Architecture

### Core Design Principles
- **No direct `Instantiate` calls** — always use a factory (`IFactory`/`BaseFactory`). This allows swapping `InstantiateFactory`, `ZenjectInstantiateFactory`, or `Pool` via DI bindings without touching callsites.
- **No direct `Destroy` calls** — use `IDestroyHandler` so the destruction strategy is delegated.
- All major systems are **interface-first**: inject the interface, bind the implementation in a Zenject installer.

### Modules

**Factories** (`Runtime/Factories/`)
`BaseFactory` handles creation + initialization via `RootInitializer` on the prefab root. Components that need dependencies implement `IInitializable` and receive them in `Initialize()`. Use `ZenjectDependencyProvider` to feed the DI container into spawned objects.

**Storage** (`Runtime/Storage/`)
Hierarchical `IStorage` (parent/child scoping) with multiple `IStorageChannel` backends: PlayerPrefs, JSON file, AES-encrypted file, and in-memory. Compose them for different persistence needs.

**Scores** (`Runtime/Scores/`)
`IScoreStorage` (read/write with events) → `IScoreBank` (acquire/spend with guards). `DefaultScoreStorage` wraps an `IStorageChannel`. `MultipliedScoreBank` wraps a bank with `AdvancedFloat` multipliers for earning/spending rates.

**Maths** (`Runtime/Maths/`)
`AdvancedFloat` applies ordered `FloatModificator` operations (Add/Multiply) to an initial value. Modificators with order 0–9 are grouped (same-type combined); other orders apply sequentially. Call `Recalculate()` manually or pass `autoRecalculate: true` to mutation methods.

**Networking** (`Runtime/Networking/`)
Generic `ApiClient<TConfig>` built on Unity's `UnityWebRequest`. Requests are constructed fluently via `IWebRequestBuilder`. Throws typed exceptions: `BadWebRequest`, `ConnectionError`, `ServerError`.

**Builds** (`Runtime/Builds/`)
`AppConfiguration` is a `ScriptableObject` loaded from `Resources/`. Use `AppConfiguration.GetMode()` for environment branching, `AppConfiguration.IsDev()` as a shorthand, and `AppConfiguration.IsDebugUIVisible()` to gate debug UI.

**Serialization** (`Runtime/Serialization/`)
`ISerializer` abstraction over `JsonUtilitySerializer` (built-in Unity) and `NewtonsoftJsonSerializer`. Prefer injecting `ISerializer` rather than calling serializers directly.

**UI** (`Runtime/UI/`)
`BaseScreenManager` manages `IScreen` objects identified by GUID. `MultiGraphicsButton` drives multiple graphic targets from a single button state.

**Identifiers** (`Runtime/Identifiers/`)
GUID-based `ScriptableObject` assets (`GuidAsset`) with a custom `GuidAssetDrawer` inspector for stable cross-scene references.

**Files** (`Runtime/Files/`)
Abstraction over Unity file locations (StreamingAssets, PersistentDataPath). Used internally by JSON and encrypted storage backends.

Do not use '[Inject]' for properties or fields, for any injecting use method Construct with [Inject] attribute
