# PathX.ChangeExtension

Target APIs:

```csharp
public sealed class PathX
{
    public PathX(PathXOptions options);
    public PathXOptions Options { get; }

    [return: NotNullIfNotNull(nameof(path))]
    public static string? ChangeExtension(string? path, string? newExtension, PathXOptions options);

    [return: NotNullIfNotNull(nameof(path))]
    public string? ChangeExtension(string? path, string? newExtension);
}
```

References:
- Requirements: `requirements/path-analysis.md` (ChangeExtension is separator-sensitive)
- Options: `src/Light.ExtendedPath/PathXOptions.cs`

## Original API from System.IO.Path

```csharp
[return: NotNullIfNotNull(nameof(path))]
public static string? ChangeExtension(string? path, string? extension)
{
    if (path == null)
        return null;

    int subLength = path.Length;
    if (subLength == 0)
        return string.Empty;

    for (int i = path.Length - 1; i >= 0; i--)
    {
        char ch = path[i];

        if (ch == '.')
        {
            subLength = i;
            break;
        }

        if (PathInternal.IsDirectorySeparator(ch))
        {
            break;
        }
    }

    if (extension == null)
    {
        return path.Substring(0, subLength);
    }

    ReadOnlySpan<char> subpath = path.AsSpan(0, subLength);
    return extension.StartsWith('.') ?
        string.Concat(subpath, extension) :
        string.Concat(subpath, ".", extension);
}
```

## Goals

- Implement a rules-driven equivalent of `System.IO.Path.ChangeExtension` that respects directory separators defined by `PathXOptions`.
- Preserve .NET semantics for null/empty `newExtension` and special name handling.
- Keep behavior pure string-based (no filesystem I/O).

## Semantics (aligned to System.IO.Path)

- If `path` is `null` → return `null`.
- If `path.Length == 0` → return `string.Empty`.
- Find the last `'.'` in the last path segment (scan backward until a directory separator is encountered). If found, set `subLength` to that index; else `subLength = path.Length`.
- If `newExtension == null` → return `path.Substring(0, subLength)` (i.e., remove extension when present; unchanged when no extension).
- Else, build from `subpath = path[..subLength]` and `newExtension`:
  - If `newExtension` starts with `'.'` → return `subpath + newExtension`.
  - Else → return `subpath + "." + newExtension`.
- This mirrors the stock implementation (notably: `newExtension == string.Empty` results in a trailing dot).

Notes:
- Only the last extension is affected (e.g., `"archive.tar.gz"` with `".zip"` → `"archive.tar.zip"`).
- Trailing separators are allowed; the new extension is appended after the entire `path` if no dot is found in the last segment (same as BCL behavior).

## Options interplay (PathXOptions)

- __RecognizedSeparators__: Used to detect boundaries of the last segment. Implement `IsDirectorySeparator(char)` by delegating to `options.IsDirectorySeparator(c)`.
- __PreferredSeparator__, __VolumeSeparator__, __IsSupportingDriveLetters__, __IsSupportingUncPaths__, __IsSupportingDevicePaths__, __CollapseSeparatorRuns__, __PreserveSeparatorsAfterRoot__, __TrimTrailingSeparatorsExceptRoot__, __IsCaseSensitive__: Not used by this method’s logic.

Rationale: `ChangeExtension` operates only on the last segment and does not generate separators nor compare names.

## Edge cases and behaviors

- __Leading-dot files__: `.bashrc` + `null` → `""`; `.bashrc` + `".txt"` → `".txt"`.
- __Special names__: `"."`, `".."` contain dots but are treated by the algorithm as normal characters when scanning; result aligns with BCL (e.g., `"."` + `".txt"` → `".txt"`). We will document this in README.
- __Trailing separator__: `"dir/"` + `".txt"` (Unix rules) → `"dir/.txt"`.
- __Roots__: `"/"` (Unix) or `"C:\\"` (Windows rules) + `".txt"` → the algorithm appends based on scan result (likely `"/.txt"`, `"C:\\.txt"`). This matches the stock behavior; document if we later decide to deviate.
- __Backslash on Unix__: With `PathXOptions.Unix`, `'\\'` is NOT a separator (backslash can be part of the file name). Scanning continues past `'\\'` until `'/'`.

All of the above are consistent with BCL’s separator interpretation per platform; here it’s driven by `PathXOptions`.

## API contract and validation

- Static API: Throw `ArgumentNullException` if `options` is `null`.
- Instance API:
  - `new PathX(PathXOptions options)`: throw `ArgumentNullException` if `options` is `null`; store in `Options`.
  - `ChangeExtension(string? path, string? newExtension)`: uses the bound `Options` and otherwise follows the same semantics as the static method.
- No other argument exceptions (APIs are tolerant of `null`/empty `path` and `newExtension` by design).
- Attribute `[return: NotNullIfNotNull(nameof(path))]` on both APIs to mirror BCL.

## Algorithm (single pass backward scan)

1. Handle `path == null` → return null.
2. If `path.Length == 0` → return string.Empty.
3. Initialize `subLength = path.Length`.
4. For `i = path.Length - 1` down to `0`:
   - `ch = path[i]`
   - If `ch == '.'` → `subLength = i`; `break`.
   - If `options.IsDirectorySeparator(ch)` → `break` (stop scanning at last separator boundary).
5. If `newExtension == null`: return `path.Substring(0, subLength)`.
6. `ReadOnlySpan<char> subpath = path.AsSpan(0, subLength)`.
7. If `newExtension.Length > 0` and `newExtension[0] == '.'` → return `string.Concat(subpath, newExtension)`.
8. Else → return `string.Concat(subpath, ".", newExtension)`.

Complexity: O(n) time, O(k) additional allocation for the resulting string (concat); no intermediate allocations beyond the result.

## Test plan

- __Null and empty__:
  - `(null, any, options)` → `null`
  - `("", any, options)` → `""`
- __Replace extension__:
  - `("file.txt", ".md")` → `"file.md"`
  - `("dir/file.txt", "md")` (Unix) → `"dir/file.md"`
  - `("dir\\file.txt", ".md")` (Windows) → `"dir\\file.md"`
- __Remove extension__:
  - `("file.txt", null)` → `"file"`
  - `("archive.tar.gz", null)` → `"archive.tar"`
- __Empty newExtension__ (trailing dot):
  - `("file.txt", "")` → `"file."`
- __No dot in last segment__:
  - `("file", ".txt")` → `"file.txt"`
  - `("a.b/c", ".txt")` (Unix) → `"a.b/c.txt"`
- __Trailing separator__:
  - `("dir/", ".txt")` (Unix) → `"dir/.txt"`
  - `("dir\\", ".txt")` (Windows) → `"dir\\.txt"`
- __Leading dot names__:
  - `(".bashrc", null)` → `""`
  - `(".bashrc", ".txt")` → `".txt"`
- __Backslash-on-Unix semantics__:
  - `("a\\b.txt", null, Unix)` → removes extension (`"a\\b"`) treating backslash as regular char.
- __Instance API parity__:
  - For each test above, also verify `new PathX(options).ChangeExtension(path, newExtension)` produces the same result as the static API.

Each case should be run with at least `PathXOptions.Unix` and `PathXOptions.Windows`; add `PathXOptions.WindowsUncPaths` variants for separator parsing coverage.

## Implementation notes

- Use `options.IsDirectorySeparator(c)` exclusively; never rely on `System.IO.PathInternal` or `Path.DirectorySeparatorChar`.
- Do not normalize separators; do not change case; do not trim trailing separators.
- Keep behavior identical to BCL apart from separator rules (which are driven by options).
- Add XML docs summarizing the options dependency and any intentional deviations.
- Type shape: make `PathX` a non-static sealed class that can host both static and instance APIs; keep existing static presets as static properties on the same type.
- Implementation structure:
  - Factor core logic into a single private helper (e.g., `ChangeExtensionCore(string? path, string? newExtension, PathXOptions options)`).
  - Static method calls the helper directly.
  - Instance method calls the same helper passing `this.Options`.

## Non-goals (for this method)

- No span-based overloads in this iteration (can be added later as allocation-free variants).
- No normalization of separators or roots.
- No device/UNC parsing beyond recognizing separators.
