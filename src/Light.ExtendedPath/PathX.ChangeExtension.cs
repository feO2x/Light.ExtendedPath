using System;
using System.Diagnostics.CodeAnalysis;
using Light.GuardClauses;

namespace Light.ExtendedPath;

public sealed partial class PathX
{
    /// <summary>
    /// Changes the extension of the specified path using the provided <see cref="PathXOptions" /> to
    /// determine directory separators. Semantics mirror <see cref="System.IO.Path.ChangeExtension(string?, string?)" />
    /// except that separator recognition is driven by <paramref name="options" />.
    /// </summary>
    /// <param name="path">The path to modify.</param>
    /// <param name="newExtension">The new extension. May start with a dot or be null/empty.</param>
    /// <param name="options">Options that define which characters are treated as directory separators.</param>
    /// <returns>The modified path, or null if <paramref name="path" /> is null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options" /> is null.</exception>
    [return: NotNullIfNotNull(nameof(path))]
    public static string? ChangeExtension(string? path, string? newExtension, PathXOptions options)
    {
        options.MustNotBeNull();
        return ChangeExtensionCore(path, newExtension, options);
    }

    /// <summary>
    /// Changes the extension of the specified path using this instance's <see cref="Options" />.
    /// </summary>
    /// <param name="path">The path to modify.</param>
    /// <param name="newExtension">The new extension. May start with a dot or be null/empty.</param>
    /// <returns>The modified path, or null if <paramref name="path" /> is null.</returns>
    [return: NotNullIfNotNull(nameof(path))]
    public string? ChangeExtension(string? path, string? newExtension) =>
        ChangeExtensionCore(path, newExtension, Options);

    private static string? ChangeExtensionCore(string? path, string? newExtension, PathXOptions options)
    {
        if (path is null)
        {
            return null;
        }

        if (path.Length == 0)
        {
            return string.Empty;
        }

        var subLength = path.Length;
        for (var i = path.Length - 1; i >= 0; i--)
        {
            var ch = path[i];
            if (ch == '.')
            {
                subLength = i;
                break;
            }

            if (options.IsDirectorySeparator(ch))
            {
                break;
            }
        }

        if (newExtension is null)
        {
            return path.Substring(0, subLength);
        }

        var subpath = path.AsSpan(0, subLength);
        return newExtension.Length > 0 && newExtension[0] == '.' ?
            string.Concat(subpath, newExtension) :
            string.Concat(subpath, ".", newExtension);
    }
}
