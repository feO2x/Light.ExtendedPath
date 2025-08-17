using FluentAssertions;
using Xunit;

namespace Light.ExtendedPath;

public static class PathXChangeExtensionTests
{
    public static TheoryData<string?, string?, PathXOptions, string?> NullPathReturnsNullCases => new ()
    {
        { null, ".txt", PathXOptions.Unix, null },
        { null, null, PathXOptions.Windows, null }
    };

    public static TheoryData<string?, string?, PathXOptions, string?> EmptyPathReturnsEmptyCases => new ()
    {
        { string.Empty, ".txt", PathXOptions.Unix, string.Empty },
        { string.Empty, null, PathXOptions.Windows, string.Empty }
    };

    public static TheoryData<string, string?, PathXOptions, string> ReplaceExtensionBasicCases => new ()
    {
        { "file.txt", ".md", PathXOptions.Unix, "file.md" },
        { "dir/file.txt", "md", PathXOptions.Unix, "dir/file.md" },
        { "dir\\file.txt", ".md", PathXOptions.Windows, "dir\\file.md" }
    };

    public static TheoryData<string, string?, PathXOptions, string> RemoveExtensionCases => new ()
    {
        { "file.txt", null, PathXOptions.Unix, "file" },
        { "archive.tar.gz", null, PathXOptions.Unix, "archive.tar" }
    };

    public static TheoryData<string, string?, PathXOptions, string> NoDotInLastSegmentCases => new ()
    {
        { "file", ".txt", PathXOptions.Unix, "file.txt" },
        { "a.b/c", ".txt", PathXOptions.Unix, "a.b/c.txt" }
    };

    public static TheoryData<string, string?, PathXOptions, string> TrailingSeparatorCases => new ()
    {
        { "dir/", ".txt", PathXOptions.Unix, "dir/.txt" },
        { "dir\\", ".txt", PathXOptions.Windows, "dir\\.txt" }
    };

    public static TheoryData<string, string?, PathXOptions, string> LeadingDotFilesCases => new ()
    {
        { ".bashrc", null, PathXOptions.Unix, string.Empty },
        { ".bashrc", ".txt", PathXOptions.Unix, ".txt" }
    };

    public static TheoryData<string, string?, PathXOptions, string> WindowsUncPathsScanRespectsSeparatorsCases => new ()
    {
        // Last segment ends after last separator recognized by options
        { "a/b.txt", null, PathXOptions.WindowsUncPaths, "a/b" },
        { "a\\b.txt", null, PathXOptions.WindowsUncPaths, "a\\b" },
        { "a/b\\c.txt", null, PathXOptions.WindowsUncPaths, "a/b\\c" }
    };

    private static void AssertBothApis(string? path, string? newExtension, PathXOptions options, string? expected)
    {
        // static API
        var staticResult = PathX.ChangeExtension(path, newExtension, options);

        // instance API
        var pathX = new PathX(options);
        var instanceResult = pathX.ChangeExtension(path, newExtension);

        // expectation
        staticResult.Should().Be(expected);

        // parity
        instanceResult.Should().Be(staticResult, "instance API must mirror static API");
    }

    // Null and empty
    [Theory]
    [MemberData(nameof(NullPathReturnsNullCases))]
    public static void NullPath_ReturnsNull(
        string? path,
        string? newExtension,
        PathXOptions options,
        string? expected
    ) =>
        AssertBothApis(path, newExtension, options, expected);

    [Theory]
    [MemberData(nameof(EmptyPathReturnsEmptyCases))]
    public static void EmptyPath_ReturnsEmpty(
        string? path,
        string? newExtension,
        PathXOptions options,
        string? expected
    ) =>
        AssertBothApis(path, newExtension, options, expected);

    // Replace extension
    [Theory]
    [MemberData(nameof(ReplaceExtensionBasicCases))]
    public static void ReplaceExtension_Basic(
        string path,
        string? newExtension,
        PathXOptions options,
        string expected
    ) =>
        AssertBothApis(path, newExtension, options, expected);

    // Remove extension
    [Theory]
    [MemberData(nameof(RemoveExtensionCases))]
    public static void RemoveExtension(string path, string? newExtension, PathXOptions options, string expected) =>
        AssertBothApis(path, newExtension, options, expected);

    // Empty newExtension (trailing dot)
    [Theory]
    [InlineData("file.txt", "", nameof(PathXOptions.Unix), "file.")]
    public static void EmptyNewExtension_YieldsTrailingDot(
        string path,
        string? newExtension,
        string optionsName,
        string expected
    )
    {
        var options = optionsName == nameof(PathXOptions.Unix) ? PathXOptions.Unix : PathXOptions.Windows;
        AssertBothApis(path, newExtension, options, expected);
    }

    // No dot in last segment
    [Theory]
    [MemberData(nameof(NoDotInLastSegmentCases))]
    public static void NoDotInLastSegment(string path, string? newExtension, PathXOptions options, string expected) =>
        AssertBothApis(path, newExtension, options, expected);

    // Trailing separator
    [Theory]
    [MemberData(nameof(TrailingSeparatorCases))]
    public static void TrailingSeparator(string path, string? newExtension, PathXOptions options, string expected) =>
        AssertBothApis(path, newExtension, options, expected);

    // Leading dot names
    [Theory]
    [MemberData(nameof(LeadingDotFilesCases))]
    public static void LeadingDotFiles(string path, string? newExtension, PathXOptions options, string expected) =>
        AssertBothApis(path, newExtension, options, expected);

    // Backslash-on-Unix semantics
    [Theory]
    [InlineData("a\\b.txt", null, nameof(PathXOptions.Unix), "a\\b")]
    public static void BackslashIsNotSeparatorOnUnix(
        string path,
        string? newExtension,
        string optionsName,
        string expected
    )
    {
        var options = optionsName == nameof(PathXOptions.Unix) ? PathXOptions.Unix : PathXOptions.Windows;
        AssertBothApis(path, newExtension, options, expected);
    }

    // Separator parsing coverage with WindowsUncPaths
    [Theory]
    [MemberData(nameof(WindowsUncPathsScanRespectsSeparatorsCases))]
    public static void WindowsUncPaths_ScanRespectsSeparators(
        string path,
        string? newExtension,
        PathXOptions options,
        string expected
    ) =>
        AssertBothApis(path, newExtension, options, expected);
}
