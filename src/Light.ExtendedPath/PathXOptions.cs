using System;
using System.Collections.Immutable;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;

namespace Light.ExtendedPath;

/// <summary>
/// Represents options that control how path operations are performed,
/// including which separators are recognized, preferred output separators,
/// and normalization behavior.
/// </summary>
public sealed record PathXOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PathXOptions" /> class.
    /// </summary>
    /// <param name="recognizedSeparators">Characters recognized as directory separators.</param>
    /// <param name="preferredSeparator">The preferred separator for output paths.</param>
    /// <param name="volumeSeparator">The volume separator character, or null if not supported.</param>
    /// <param name="isSupportingDriveLetters">Whether drive letters are supported.</param>
    /// <param name="isSupportingUncPaths">Whether UNC paths are supported.</param>
    /// <param name="isSupportingDevicePaths">Whether device paths are supported.</param>
    /// <param name="isCaseSensitive">Whether path comparisons should be case-sensitive.</param>
    /// <param name="collapseSeparatorRuns">Whether to collapse runs of separators.</param>
    /// <param name="preserveSeparatorsAfterRoot">Whether to preserve separator runs after a root.</param>
    /// <param name="trimTrailingSeparatorsExceptRoot">Whether to trim trailing separators except in root paths.</param>
    /// <exception cref="EmptyCollectionException">Thrown when <paramref name="recognizedSeparators" /> is empty or the default instance.</exception>
    /// <exception cref="MissingItemException">Thrown when <paramref name="recognizedSeparators" /> does not contain <paramref name="preferredSeparator" />.</exception>
    public PathXOptions(
        ImmutableArray<char> recognizedSeparators,
        char preferredSeparator,
        char? volumeSeparator,
        bool isSupportingDriveLetters,
        bool isSupportingUncPaths,
        bool isSupportingDevicePaths,
        bool isCaseSensitive,
        bool collapseSeparatorRuns,
        bool preserveSeparatorsAfterRoot,
        bool trimTrailingSeparatorsExceptRoot
    )
    {
        RecognizedSeparators = recognizedSeparators
           .MustNotBeDefaultOrEmpty()
           .MustContain(preferredSeparator, nameof(recognizedSeparators));
        PreferredSeparator = preferredSeparator;
        VolumeSeparator = volumeSeparator;
        IsSupportingDriveLetters = isSupportingDriveLetters;
        IsSupportingUncPaths = isSupportingUncPaths;
        IsSupportingDevicePaths = isSupportingDevicePaths;
        IsCaseSensitive = isCaseSensitive;
        CollapseSeparatorRuns = collapseSeparatorRuns;
        PreserveSeparatorsAfterRoot = preserveSeparatorsAfterRoot;
        TrimTrailingSeparatorsExceptRoot = trimTrailingSeparatorsExceptRoot;
    }

    /// <summary>
    /// Gets the default Windows path options.
    /// </summary>
    public static PathXOptions Windows { get; } = new (
        recognizedSeparators: ['\\', '/'],
        preferredSeparator: '\\',
        volumeSeparator: ':',
        isSupportingDriveLetters: true,
        isSupportingUncPaths: true,
        isSupportingDevicePaths: true,
        isCaseSensitive: false,
        collapseSeparatorRuns: true,
        preserveSeparatorsAfterRoot: true,
        trimTrailingSeparatorsExceptRoot: true
    );

    /// <summary>
    /// Gets the default Unix path options.
    /// </summary>
    public static PathXOptions Unix { get; } = new (
        recognizedSeparators: ['/'],
        preferredSeparator: '/',
        volumeSeparator: null,
        isSupportingDriveLetters: false,
        isSupportingUncPaths: false,
        isSupportingDevicePaths: false,
        isCaseSensitive: true,
        collapseSeparatorRuns: true,
        preserveSeparatorsAfterRoot: true,
        trimTrailingSeparatorsExceptRoot: true
    );

    /// <summary>
    /// Windows UNC path semantics intended for use on both Windows and Unix hosts.
    /// This preset recognizes both '\\' and '/', supports drive letters, UNC and device paths, uses
    /// case-insensitive comparisons like Windows, and will normalize output to '\' to keep the UNC format consistent.
    /// These options are not suitable for device UNC paths.
    /// </summary>
    public static PathXOptions WindowsUncPaths { get; } = new (
        recognizedSeparators: ['\\', '/'],
        preferredSeparator: '\\',
        volumeSeparator: null,
        isSupportingDriveLetters: false,
        isSupportingUncPaths: true,
        isSupportingDevicePaths: false,
        isCaseSensitive: false,
        collapseSeparatorRuns: true,
        preserveSeparatorsAfterRoot: true,
        trimTrailingSeparatorsExceptRoot: true
    );

    /// <summary>
    /// Gets path options that match the current operating system.
    /// </summary>
    public static PathXOptions ForCurrentOperatingSystem =>
        OperatingSystem.IsWindows() ? Windows : Unix;

    /// <summary>
    /// Gets a set of characters recognized as directory separators.
    /// </summary>
    public ImmutableArray<char> RecognizedSeparators { get; }

    /// <summary>
    /// Gets the preferred separator character used when generating paths.
    /// </summary>
    public char PreferredSeparator { get; }

    /// <summary>
    /// Gets the volume separator character (e.g., ':' in "C:\" on Windows), or null if not supported.
    /// </summary>
    public char? VolumeSeparator { get; }

    /// <summary>
    /// Gets a value indicating whether drive letters (e.g., "C:") are supported.
    /// </summary>
    public bool IsSupportingDriveLetters { get; }

    /// <summary>
    /// Gets a value indicating whether UNC paths (e.g., "\\server\share") are supported.
    /// </summary>
    public bool IsSupportingUncPaths { get; }

    /// <summary>
    /// Gets a value indicating whether device paths (e.g., "\\?\C:\" or "\\.\COM1") are supported.
    /// </summary>
    public bool IsSupportingDevicePaths { get; }

    /// <summary>
    /// Gets a value indicating whether path comparisons should be case-sensitive.
    /// </summary>
    public bool IsCaseSensitive { get; }

    /// <summary>
    /// Gets a value indicating whether runs of separators should be collapsed into a single separator.
    /// </summary>
    public bool CollapseSeparatorRuns { get; }

    /// <summary>
    /// Gets a value indicating whether separator runs should be preserved after a root
    /// (e.g., in UNC paths like "\\server\share").
    /// </summary>
    public bool PreserveSeparatorsAfterRoot { get; }

    /// <summary>
    /// Gets a value indicating whether trailing separators should be trimmed,
    /// except when they are part of a root path.
    /// </summary>
    public bool TrimTrailingSeparatorsExceptRoot { get; }

    /// <summary>
    /// Gets the string comparison to use for path operations based on case sensitivity.
    /// </summary>
    public StringComparison StringComparison =>
        IsCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

    /// <summary>
    /// Determines whether the specified character is recognized as a directory separator.
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <returns>true if the character is a recognized separator; otherwise, false.</returns>
    public bool IsDirectorySeparator(char c) => RecognizedSeparators.Contains(c);
}
