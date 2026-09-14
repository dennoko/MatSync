# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

MatSync is an Editor-only Unity extension (Unity 2022.3.22f1, lilToon 2.3.4) for sharing settings across lilToon materials. It has three modes: **Copy** (one source → many targets), **Compare** (block-by-block left/right merge in a separate window), and **Sync** (one-way live sync of source edits made in the regular Material Inspector). It opens from **Tools → dennokoworks → MatSync**.

This folder is the git repo root, but it sits inside a VCC Unity project at `Assets/dennokoworks/MatSync`. Hard-coded fallback asset paths (`Assets/dennokoworks/MatSync/Editor/UI/*.uss|uxml`) depend on that location. Docs, UI strings, and comments are in Japanese.

## Commands

There are no automated tests. Verification means compiling standalone with Unity's bundled Roslyn, then checking manually in Unity:

```powershell
& ./Tools~/compile.ps1                                 # compiles every Editor/**/*.cs into Temp~/Compile (warnings are errors)
& ./Tools~/compile.ps1 -UnityEditor 'D:/Unity/2022.3.22f1/Editor'   # if Unity is installed somewhere else
```

The script builds against the real UnityEngine/UnityEditor DLLs and .NET Standard 2.1 with `-langversion:9.0 -warnaserror+`, so keep code C# 9 compatible. The script doesn't launch Unity or touch assets. Output and `compile.log` go to `Temp~/Compile`, which Unity ignores because of the `~` suffix.

Regenerate the property allowlist (`Editor/PropertyCatalog.cs`) and the support table (`Docs/Impl/property-support.md`) from a lilToon package:

```powershell
python Tools~/generate_catalog.py <path to lilToon package dir>
```

It refuses to run on any version except 2.3.4. Supporting a new lilToon version means reviewing the `block()` and `exclusion()` classification rules in the script first. Don't hand-edit `PropertyCatalog.cs`.

The manual Unity test procedure, mapped to acceptance criteria AC-xx, is in `Docs/Impl/prototype-test.md`.

## Architecture

The processing logic is separate from the UI. Processing classes never draw GUI. They take Material references, property sets, and options, and they return plans and results.

- **`MaterialModel.cs`** is the core model:
  - `PropertyDefinition` / `PropertyValue` / `MaterialSnapshot`: typed property values
  - `MaterialSchema`: a per-shader definition cache, cleared on `projectChanged`
  - `LilToonBridge`: reaches lilToon's `lilToon.lilMaterialUtils` (`RemoveShaderKeywords`, `SetupMultiMaterial`) **via reflection**, so the tool still compiles without lilToon installed. Also provides `Validate()`.
  - `MaterialSelection.Extract`: turns dropped Materials or Hierarchy objects into materials using `sharedMaterials` from MeshRenderer and SkinnedMeshRenderer, including children and inactive objects.
- **`PropertyCatalog.cs`** is generated. It holds the shader-name allowlist, the property → block mapping (block names such as 影, MatCap, 透過・描画設定), and exclusion reasons. Properties missing from the catalog are excluded.
- **`MaterialTransfer.cs`** decides what can transfer and builds a `TransferPlan` holding only real diffs. It applies plans in batches and owns Undo grouping: it creates a group only when something changes, and it can join an existing group, which Sync uses.
- **`MaterialComparison.cs`** handles the Compare-mode state (`ComparisonBlock`: take left / take right / no change). It snapshots the full serialized state at start and re-checks for external changes just before applying.
- **`MaterialSyncSession.cs`** is `IDisposable`. It hooks `Undo.willFlushUndoRecord` and `Undo.undoRedoPerformed` and records sync targets into the Inspector's *current* Undo group, once per target per group. It skips syncing while `Undo.isProcessing` and re-takes the baseline after an undo or redo. The session stops, with no auto-resume, on settings or mode changes, window close, Play Mode, domain reload, or when a multi-material Inspector that includes the source is detected.
- **UI (UI Toolkit)**:
  - `MatSyncWindow.cs` and `MatSyncComparisonWindow.cs` clone UXML from `Editor/UI/` and bind elements with `root.Q<T>("name")`.
  - Each window loads `DennokoTheme.uss` (the shared dennokoworks design system) and then `MatSyncTheme.uss`, looking assets up by GUID constant with a path fallback.
  - `DennokoUIFont.Apply(root)` generates and self-repairs the Meiryo SDF font.
  - `MaterialSelectionGUI.cs` still contains the older IMGUI helpers. It also provides the UI Toolkit `SetupDropArea` / `Choose` / `Add` helpers the windows use.
  - The README and `prototype-implementation.md` still describe the UI as IMGUI; that is outdated.
- **Version check**:
  - `MatSyncVersion` (`[InitializeOnLoad]`) reads the local `version.json` by GUID and calls `DennokoVersionChecker`.
  - The checker fetches `version.json` from `raw.githubusercontent.com/dennoko/MatSync/<branch>`.
  - Results are cached in `EditorPrefs` and `SessionState` and refreshed at most every 6 hours.
  - To release, bump `version.json`.

## Transfer rules to preserve

- Textures (reference + Tiling + Offset, treated as one unit) are **kept by default** in every mode. They transfer only when the "テクスチャを含める" (include textures) option is on.
- Shader, Render Queue, and Override Tags are never copied. Settings that need a shader or render-mode change (`_TransparentMode`, `_UseOutline`, `_UseClippingCanceller`, `_AsOverlay`) are excluded.
- The 透過・描画設定 (transparency/render settings) block transfers only between identical shaders. Other blocks transfer across different allowlisted shaders when the property name and type match. Float and Range may convert to each other; Int is distinct.
- Hidden gradient-edit keys (`_egc*`, `_e2g*`, etc.) and `_lilToonVersion` are not transferred. `_BaseColor` / `_BaseMap` / `_BaseColorMap` are derived values.
- After applying, call `RemoveShaderKeywords` for normal variants or `SetupMultiMaterial` for lilToonMulti.
- Mark changed materials dirty, but don't call `SaveAssets` per update and don't scan the whole project.

## Docs

`Docs/Impl/requirements.md` holds the detailed requirements (IDs such as MAT-, PROP-, COPY-, COMP-, SYNC-, UI-, NFR-). `prototype-implementation.md` maps files to requirements, and `overview.md` records the original concept. For UI work, the `dennokoworks_color_schema` skill defines the UI Toolkit design system that `DennokoTheme.uss` implements.
