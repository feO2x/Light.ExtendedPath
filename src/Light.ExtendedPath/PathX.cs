using System;
using Light.GuardClauses;

namespace Light.ExtendedPath;

/// <summary>
/// Provides methods for processing file system strings in a cross-platform manner,
/// with explicit control over directory separators and path rules.
/// </summary>
public sealed partial class PathX
{
    /// <summary>
    /// Initializes a new instance of <see cref="PathX" /> with the specified <see cref="PathXOptions" />.
    /// </summary>
    /// <param name="options">The options controlling separator recognition and path rules.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options" /> is null.</exception>
    public PathX(PathXOptions options)
    {
        Options = options.MustNotBeNull();
    }

    /// <summary>
    /// Gets the options used by this instance.
    /// </summary>
    public PathXOptions Options { get; }
}
