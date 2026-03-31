# Localization Module

This folder contains a lightweight localization pipeline for Unity projects that need:

- CSV-driven authoring for designers and translators
- binary export for runtime loading
- integer key lookup for stable, fast access
- low-overhead formatting paths for high-frequency UI

The implementation is split into `Runtime` and `Editor` code so the runtime side stays focused on loading and lookup while the editor side handles CSV parsing and asset generation.

## Folder Layout

```text
Assets/
  Localization/
    CSV/
      i18n_text.csv
  Resources/
    Localization/
      zh-CN.bytes
      en.bytes
      ja.bytes
  Scripts/
    Localization/
      README.md
      Editor/
        CsvUtility.cs
        LocalizationBinaryExporter.cs
      Generated/
        I18nKey.cs
      Runtime/
        FastNumberFormatter.cs
        LocalizationBootstrap.cs
        LocalizationFastFormatter.cs
        LocalizationManager.cs
        LocalizationTableLoader.cs
        LocalizedText.cs
```

## Source Data

The source of truth is:

- [i18n_text.csv](../../../Localization/CSV/i18n_text.csv)

Recommended CSV schema:

```csv
id,key,module,zh-CN,en,ja,comment
1001,ui_login,UI,登录,Login,ログイン,登录按钮
1002,ui_cancel,UI,取消,Cancel,キャンセル,取消按钮
1003,ui_mail_count,Mail,你有 {0} 封邮件,You have {0} mails,メールが {0} 件あります,邮件数量提示
1004,error_network,Basic,网络异常,Network Error,ネットワークエラー,通用错误
```

Column rules:

- `id`: unique integer runtime key
- `key`: unique semantic identifier used for code generation
- `module`: optional grouping field for ownership or future splitting
- language columns: any non-reserved column becomes an exported language table
- `comment`: optional context for translators and designers

Reserved columns:

- `id`
- `key`
- `module`
- `comment`

All other columns are treated as language codes.

## Export Workflow

Use the Unity editor menu:

- `Tools/Localization/Export Binary Tables`

Exporter entry point:

- [LocalizationBinaryExporter.cs](Editor/LocalizationBinaryExporter.cs)

What the exporter does:

1. Reads `Assets/Localization/CSV/i18n_text.csv`
2. Validates required columns
3. Validates duplicate `id` and duplicate `key`
4. Treats each language column as an output table
5. Writes one `.bytes` file per language into `Assets/Resources/Localization`
6. Generates `Assets/Scripts/Localization/Generated/I18nKey.cs`
7. Refreshes the AssetDatabase

Generated binary files are loaded through Unity `Resources`, so the resource path must stay aligned with:

```csharp
Resources.Load<TextAsset>($"Localization/{languageCode}");
```

If the output directory changes, update both the exporter and the loader.

## Binary Format

Binary files are intentionally simple and versioned.

Current layout:

```text
Header
- magic   : int32
- version : int32
- count   : int32

Body (repeated count times)
- id      : int32
- text    : string
```

Current constants:

- `magic = 0x4941384E`
- `version = 1`

Reader:

- [LocalizationTableLoader.cs](Runtime/LocalizationTableLoader.cs)

Writer:

- [LocalizationBinaryExporter.cs](Editor/LocalizationBinaryExporter.cs)

If you evolve the format in the future:

1. bump `version`
2. update exporter and runtime loader together
3. keep backward compatibility only if the project requires loading old assets

## Runtime Components

### LocalizationManager

Primary runtime service:

- [LocalizationManager.cs](Runtime/LocalizationManager.cs)

Responsibilities:

- initialize current and fallback language
- load binary tables from `Resources`
- perform integer-key lookup
- notify listeners when language changes
- provide regular and fast formatting entry points

Main APIs:

```csharp
LocalizationManager.Instance.Initialize("zh-CN", "zh-CN");
LocalizationManager.Instance.SetLanguage("en");
LocalizationManager.Instance.Get(I18nKey.UI_LOGIN);
LocalizationManager.Instance.FormatParams(id, a, b, c);
LocalizationManager.Instance.Format(id, value1, value2);
LocalizationManager.Instance.FastFormat(id, value1, value2);
```

Lookup behavior:

1. query current language table
2. if missing or empty, query fallback language table
3. if still missing, return `#id`

### LocalizationBootstrap

Scene bootstrap helper:

- [LocalizationBootstrap.cs](Runtime/LocalizationBootstrap.cs)

Usage:

- add it to a startup scene object
- configure `defaultLanguage`
- configure `fallbackLanguage`

This keeps initialization explicit and scene-driven.

### LocalizedText

Simple UI binding for `UnityEngine.UI.Text`:

- [LocalizedText.cs](Runtime/LocalizedText.cs)

Behavior:

- subscribes on `OnEnable`
- refreshes on language change
- avoids redundant assignment when the resolved string did not change

Current scope:

- supports `UnityEngine.UI.Text`
- uses integer `keyId`

If the project uses TextMeshPro, add a TMP-specific equivalent instead of modifying this component in-place unless the codebase wants a combined abstraction.

## Formatting Paths

There are three formatting tiers in this module.

### 1. Plain lookup

Use `Get(id)` when the text is static:

```csharp
var title = LocalizationManager.Instance.Get(I18nKey.UI_LOGIN);
```

This is the cheapest path and should cover most labels.

### 2. General formatting

Use `FormatParams` or generic `Format(...)` overloads when call frequency is low or moderate:

```csharp
var message = LocalizationManager.Instance.Format(I18nKey.UI_MAIL_COUNT, mailCount);
var fallback = LocalizationManager.Instance.FormatParams(id, a, b, c, d);
```

Notes:

- `FormatParams` uses `params object[]`
- generic `Format` avoids the `params` array allocation at the callsite
- `string.Format(...)` may still box value types

### 3. Fast formatting

Use `FastFormat(...)` for high-frequency UI when templates are simple:

```csharp
var text = LocalizationManager.Instance.FastFormat(I18nKey.UI_MAIL_COUNT, mailCount);
```

Implementation:

- [LocalizationFastFormatter.cs](Runtime/LocalizationFastFormatter.cs)

Current fast formatter constraints:

- supports `{0}`, `{1}`, `{2}`
- supports `{{` escape for literal `{`
- does not support full `string.Format` format specifiers like `{0:N0}` or `{0:D2}`
- provides dedicated overloads for hot paths with `int`, `float`, and `string`

If you need complex formatting syntax, keep using regular `Format`.

## Fast Number Formatting

For hot numeric UI, use:

- [FastNumberFormatter.cs](Runtime/FastNumberFormatter.cs)

Purpose:

- replace frequent `ToString("N0")` calls in damage, currency, score, or stat UI

Available APIs:

```csharp
var text = FastNumberFormatter.FormatN0(1234567);   // 1,234,567
FastNumberFormatter.PrewarmN0(99999);
```

Strategy:

- prewarms a cache for a configurable non-negative range
- returns cached strings for hot small values
- falls back to a manual thousands-separator formatter for larger values

Important behavior:

- output uses `,` as the separator
- output is not culture-aware
- this is intentional for deterministic and lower-overhead runtime formatting

If you need culture-aware number formatting, do not replace `ToString("N0")` with this utility blindly.

## Maintenance Guidelines

### When adding a new language

1. add a new language column to `i18n_text.csv`
2. run `Tools/Localization/Export Binary Tables`
3. verify a new `.bytes` file is created
4. set the new language code in your game settings or bootstrap path

### When adding a new text key

1. add a new row to `i18n_text.csv`
2. choose a unique `id`
3. choose a unique `key`
4. fill in at least the fallback language text
5. export again so `I18nKey.cs` is regenerated

### When renaming a key

Renaming the `key` changes the generated constant name in `I18nKey.cs` but does not change runtime lookup if `id` stays the same.

That means:

- runtime data remains stable if `id` does not change
- code references to the generated constant name will need to be updated

### When changing an id

Avoid changing an `id` after other systems start referencing it.

Changing `id` impacts:

- runtime lookup
- serialized references if you store ids in scene or prefab components
- any code using generated constants

Prefer keeping `id` stable permanently.

### When changing the binary schema

Touch these files together:

- [LocalizationBinaryExporter.cs](Editor/LocalizationBinaryExporter.cs)
- [LocalizationTableLoader.cs](Runtime/LocalizationTableLoader.cs)

And usually update this document as well.

## Known Constraints

- `LocalizedText` currently targets `UnityEngine.UI.Text`, not TMP
- CSV validation is intentionally basic and does not yet validate placeholder compatibility across languages
- runtime loading currently uses `Resources`
- fast formatter only supports placeholder indexes `0` through `2`
- fast number formatter uses fixed comma grouping rather than locale-sensitive grouping

## Suggested Next Steps

If this module grows, the most useful follow-up additions are:

1. a `LocalizedTMPText` component for `TextMeshProUGUI`
2. placeholder validation across languages during export
3. a language config table for fonts and fallback chains
4. Addressables-based loading instead of `Resources`
5. editor tooling to choose keys by name instead of typing raw integer ids

## Quick Start

1. Edit [i18n_text.csv](../../../Localization/CSV/i18n_text.csv)
2. Run `Tools/Localization/Export Binary Tables`
3. Add [LocalizationBootstrap.cs](Runtime/LocalizationBootstrap.cs) to a startup object
4. Add [LocalizedText.cs](Runtime/LocalizedText.cs) to a `Text` component
5. Assign the integer key id

This keeps the authoring path simple while preserving a fast runtime lookup path.
