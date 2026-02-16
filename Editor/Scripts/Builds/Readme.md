# AppConfiguration

`AppConfiguration` is a `ScriptableObject` used to centralize environment‑specific behavior for all builds (Editor, Development, Release). It is automatically created under the `Resources` folder and loaded at runtime.

This configuration defines how the application behaves depending on where and how it is built.

---

## Properties

### **EditorAppMode**

Determines which `AppMode` should be used when the game is running inside the Unity Editor.

* Used only when `Application.isEditor == true`.
* Does not depend on build settings.

### **DevelopmentBuildAppMode**

Determines the `AppMode` when running a **development build**.

* Active when `Debug.isDebugBuild == true`.
* Typically set to `Dev`.

### **StandardBuildAppMode**

Determines the `AppMode` when running a **non-development (release) build**.

* Active when `Debug.isDebugBuild == false`.
* Typically set to `Prod`.

### **ActivateDebugObjectsInProd**

Controls whether debug objects/UI should appear even in **production mode**.

* Useful for emergency diagnostics.
* If enabled, debug UI becomes visible in release builds.

---

## Static Methods

### **`AppConfiguration.Get()`**

Loads and returns the `AppConfiguration` asset from the `Resources` folder.

* Cached on first call.
* Use when you need access to configuration fields directly.

**Example:**

```csharp
var config = AppConfiguration.Get();
Debug.Log(config.StandardBuildAppMode);
```

---

### **`AppConfiguration.GetMode()`**

Returns the active `AppMode` according to the current execution environment:

1. If running in **Editor** → returns `EditorAppMode`
2. Else if in **Development Build** → returns `DevelopmentBuildAppMode`
3. Else → returns `StandardBuildAppMode`

**Example:**

```csharp
if (AppConfiguration.GetMode() == AppMode.Dev)
{
    // dev-specific behavior
}
```

---

### **`AppConfiguration.IsDev()`**

Shorthand helper that checks if the app is effectively running in `AppMode.Dev`.

Equivalent to:

```csharp
AppConfiguration.GetMode() == AppMode.Dev
```

**Example:**

```csharp
if (AppConfiguration.IsDev())
{
    EnableVerboseLogging();
}
```

---

### **`AppConfiguration.IsDebugUIVisible()`**

Determines if debug UI or debug-only objects should be shown.
The rule is:

* Visible when mode is **Dev**; OR
* Visible in Prod if `ActivateDebugObjectsInProd == true`.

**Example:**

```csharp
if (AppConfiguration.IsDebugUIVisible())
{
    debugPanel.SetActive(true);
}
```

---

## Usage Summary

* Use **`Get()`** to access raw configuration properties.
* Use **`GetMode()`** to branch logic based on environment.
* Use **`IsDev()`** to quickly check development behavior.
* Use **`IsDebugUIVisible()`** to control debug UI visibility globally.

This keeps environment‑specific logic clean, centralized, and consistent across your Unity project.
