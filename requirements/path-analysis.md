# System.IO.Path API — Method Inventory and Directory-Separator Sensitivity

This document inventories the public methods of `System.IO.Path` from the .NET runtime and highlights which ones are impacted by the platform’s directory separator (e.g., `\\` on Windows vs `/` on Unix).

Sources (main branch):
- Path core: https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/IO/Path.cs
- Windows partial: https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/IO/Path.Windows.cs
- Unix partials:
  - https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/IO/Path.Unix.cs
  - https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/IO/Path.Unix.iOS.cs
  - https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/IO/Path.Unix.NoniOS.cs
- PathInternal (used by Path):
  - https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/System/IO/PathInternal.cs
  - https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/System/IO/PathInternal.Windows.cs
  - https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/System/IO/PathInternal.Unix.cs
  - https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/System/IO/PathInternal.CaseSensitivity.cs

Legend for “Separator-sensitive?” column:
- Yes: Directly parses, normalizes, checks, or inserts directory separators; or semantics differ by separator.
- Indirect: Behavior depends on platform-specific path rooting/qualification rules that in turn use separators.
- No: Does not parse/emit directory separators.


## Public methods table

| Method (signature) | Defined in | Platform | Separator-sensitive? | Notes |
|---|---|---|---|---|
| ChangeExtension(string? path, string? extension) | Path.cs | All | Yes | Scans until a separator to find the last `.`; uses `PathInternal.IsDirectorySeparator`.
| Combine(params ReadOnlySpan<string> paths) | Path.cs | All | Yes | Span-based; as above.
| Combine(params string[] paths) | Path.cs | All | Yes | As above.
| Combine(string path1, string path2) | Path.cs | All | Yes | Inserts/omits separators between segments.
| Combine(string path1, string path2, string path3) | Path.cs | All | Yes | As above.
| Combine(string path1, string path2, string path3, string path4) | Path.cs | All | Yes | As above.
| EndsInDirectorySeparator(ReadOnlySpan<char> path) | Path.cs | All | Yes | Delegates to `PathInternal.EndsInDirectorySeparator`.
| EndsInDirectorySeparator(string? path) | Path.cs | All | Yes | String overload; same.
| Exists(string? path) | Path.cs (+ ExistsCore in platform files) | All | Yes | Uses `GetFullPath`, checks trailing separator vs directory.
| GetDirectoryName(ReadOnlySpan<char> path) | Path.cs | All | Yes | Span overload; does not normalize but parses separators.
| GetDirectoryName(string? path) | Path.cs | All | Yes | Computes directory portion; normalizes directory separators.
| GetExtension(ReadOnlySpan<char> path) | Path.cs | All | Yes | Span overload; same separator behavior.
| GetExtension(string? path) | Path.cs | All | Yes | Stops at directory separator when scanning for `.`.
| GetFileName(ReadOnlySpan<char> path) | Path.cs | All | Yes | Span overload; same separator behavior.
| GetFileName(string? path) | Path.cs | All | Yes | Finds last separator to slice name.
| GetFileNameWithoutExtension(ReadOnlySpan<char> path) | Path.cs | All | Yes | Span overload; same.
| GetFileNameWithoutExtension(string? path) | Path.cs | All | Yes | Based on `GetFileName`; separator-sensitive indirectly.
| GetFullPath(string path) | Path.Unix.cs | Unix | Yes | Collapses relative segments; separator-aware.
| GetFullPath(string path) | Path.Windows.cs | Windows | Yes | Normalizes, handles device/extended paths; separator-aware.
| GetFullPath(string path, string basePath) | Path.Unix.cs | Unix | Yes | Combines/roots using `/`.
| GetFullPath(string path, string basePath) | Path.Windows.cs | Windows | Yes | Combines/roots using separators.
| GetInvalidFileNameChars() | Path.Unix.cs | Unix | Yes | Set includes `/`; platform-specific invalid set.
| GetInvalidFileNameChars() | Path.Windows.cs | Windows | Yes | Set includes `\\` and `/` etc.; platform-specific invalid set.
| GetInvalidPathChars() | Path.Unix.cs | Unix | Yes | Platform-specific invalid set.
| GetInvalidPathChars() | Path.Windows.cs | Windows | Yes | Platform-specific invalid set.
| GetPathRoot(ReadOnlySpan<char> path) | Path.Unix.cs | Unix | Yes | Span overload; same.
| GetPathRoot(ReadOnlySpan<char> path) | Path.Windows.cs | Windows | Yes | Span overload; same.
| GetPathRoot(string? path) | Path.Unix.cs | Unix | Yes | `"/"` for rooted; empty/`null` otherwise.
| GetPathRoot(string? path) | Path.Windows.cs | Windows | Yes | Returns normalized root using separators.
| GetRandomFileName() | Path.cs | All | No | Generates an 8.3-like random file name; no separator logic.
| GetRelativePath(string relativeTo, string path) | Path.cs | All | Yes | Uses `DirectorySeparatorChar`; normalizes relative segments.
| GetTempFileName() | Path.Unix.cs | Unix | Yes | Creates file under temp path; separator via temp path.
| GetTempFileName() | Path.Windows.cs | Windows | Yes | Builds under temp path; relies on separator-normalized temp path.
| GetTempPath() | Path.Unix.cs (+ iOS/NoniOS) | Unix | Yes | Returns path ending with `/`.
| GetTempPath() | Path.Windows.cs | Windows | Yes | Ensures trailing directory separator.
| HasExtension(ReadOnlySpan<char> path) | Path.cs | All | Yes | Span overload; same.
| HasExtension(string? path) | Path.cs | All | Yes | Stops at directory separator when scanning for `.`.
| IsPathFullyQualified(ReadOnlySpan<char> path) | Path.cs | All | Indirect | Span overload; same.
| IsPathFullyQualified(string path) | Path.cs | All | Indirect | Delegates to `PathInternal.IsPartiallyQualified` (platform-specific rooting rules).
| IsPathRooted(ReadOnlySpan<char> path) | Path.Unix.cs | Unix | Yes | Span overload; same.
| IsPathRooted(ReadOnlySpan<char> path) | Path.Windows.cs | Windows | Yes | Span overload; same.
| IsPathRooted(string? path) | Path.Unix.cs | Unix | Yes | Rooted if starts with `/`.
| IsPathRooted(string? path) | Path.Windows.cs | Windows | Yes | Rooting with drive letters or leading separator.
| Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2) | Path.cs | All | Yes | Ensures exactly one separator between parts.
| Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3) | Path.cs | All | Yes | As above.
| Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3, ReadOnlySpan<char> path4) | Path.cs | All | Yes | As above.
| Join(params ReadOnlySpan<string?> paths) | Path.cs | All | Yes | As above.
| Join(params string?[] paths) | Path.cs | All | Yes | As above.
| Join(string? path1, string? path2) | Path.cs | All | Yes | String overloads; as above.
| Join(string? path1, string? path2, string? path3) | Path.cs | All | Yes | As above.
| Join(string? path1, string? path2, string? path3, string? path4) | Path.cs | All | Yes | As above.
| TrimEndingDirectorySeparator(ReadOnlySpan<char> path) | Path.cs | All | Yes | Span overload; same.
| TrimEndingDirectorySeparator(string path) | Path.cs | All | Yes | Delegates to `PathInternal.TrimEndingDirectorySeparator`.
| TryJoin(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3, Span<char> destination, out int charsWritten) | Path.cs | All | Yes | As above.
| TryJoin(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, Span<char> destination, out int charsWritten) | Path.cs | All | Yes | Computes need for separator; writes accordingly.

Notes:
- Fields like `DirectorySeparatorChar`, `AltDirectorySeparatorChar`, `VolumeSeparatorChar`, and `PathSeparator` are not listed here as they are constants/fields, not methods, but they are central to separator differences.
- Some helper methods in `PathInternal` are used extensively (e.g., `IsDirectorySeparator`, `GetRootLength`, `NormalizeDirectorySeparators`, `EndsInDirectorySeparator`). While internal, they explain why many `Path` methods are separator-sensitive.

## Summary
- The vast majority of `System.IO.Path` methods are separator-sensitive, either directly (parsing/inserting separators) or indirectly (rooting/qualification rules vary by OS).
- Only `GetRandomFileName()` is clearly separator-agnostic.
- Platform-specific implementations (Windows vs Unix) exist for: invalid char sets, full path resolution, temp path helpers, rooting checks, and root extraction — all tied to directory separator semantics.
