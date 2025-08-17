using System;
using System.Collections.Immutable;
using FluentAssertions;
using Light.GuardClauses.Exceptions;
using Xunit;

namespace Light.ExtendedPath;

public static class PathXOptionsTests
{
    [Fact]
    public static void WindowsPreset_ShouldHaveExpectedDefaults()
    {
        var o = PathXOptions.Windows;

        o.RecognizedSeparators.Should().BeEquivalentTo(['\\', '/']);
        o.PreferredSeparator.Should().Be('\\');
        o.VolumeSeparator.Should().Be(':');
        o.IsSupportingDriveLetters.Should().BeTrue();
        o.IsSupportingUncPaths.Should().BeTrue();
        o.IsSupportingDevicePaths.Should().BeTrue();
        o.IsCaseSensitive.Should().BeFalse();
        o.CollapseSeparatorRuns.Should().BeTrue();
        o.PreserveSeparatorsAfterRoot.Should().BeTrue();
        o.TrimTrailingSeparatorsExceptRoot.Should().BeTrue();
    }

    [Fact]
    public static void UnixPreset_ShouldHaveExpectedDefaults()
    {
        var o = PathXOptions.Unix;

        o.RecognizedSeparators.Should().BeEquivalentTo(['/']);
        o.PreferredSeparator.Should().Be('/');
        o.VolumeSeparator.Should().BeNull();
        o.IsSupportingDriveLetters.Should().BeFalse();
        o.IsSupportingUncPaths.Should().BeFalse();
        o.IsSupportingDevicePaths.Should().BeFalse();
        o.IsCaseSensitive.Should().BeTrue();
        o.CollapseSeparatorRuns.Should().BeTrue();
        o.PreserveSeparatorsAfterRoot.Should().BeTrue();
        o.TrimTrailingSeparatorsExceptRoot.Should().BeTrue();
    }

    [Fact]
    public static void ForCurrentOperatingSystem_ShouldMatchHost()
    {
        var o = PathXOptions.ForCurrentOperatingSystem;

        o.Should().BeSameAs(OperatingSystem.IsWindows() ? PathXOptions.Windows : PathXOptions.Unix);
    }

    [Fact]
    public static void WindowsUncPathsPreset_ShouldHaveExpectedDefaults()
    {
        var o = PathXOptions.WindowsUncPaths;

        o.RecognizedSeparators.Should().BeEquivalentTo(['\\', '/']);
        o.PreferredSeparator.Should().Be('\\');
        o.VolumeSeparator.Should().BeNull();
        o.IsSupportingDriveLetters.Should().BeFalse();
        o.IsSupportingUncPaths.Should().BeTrue();
        o.IsSupportingDevicePaths.Should().BeFalse();
        o.IsCaseSensitive.Should().BeFalse();
        o.CollapseSeparatorRuns.Should().BeTrue();
        o.PreserveSeparatorsAfterRoot.Should().BeTrue();
        o.TrimTrailingSeparatorsExceptRoot.Should().BeTrue();
    }

    [Fact]
    public static void StringComparison_ShouldMatchCaseSensitivity()
    {
        PathXOptions.Windows.StringComparison.Should().Be(StringComparison.OrdinalIgnoreCase);
        PathXOptions.WindowsUncPaths.StringComparison.Should().Be(StringComparison.OrdinalIgnoreCase);
        PathXOptions.Unix.StringComparison.Should().Be(StringComparison.Ordinal);
    }

    [Theory]
    [InlineData('\\')]
    [InlineData('/')]
    public static void IsDirectorySeparator_WindowsRecognizesBoth(char c)
    {
        PathXOptions.Windows.IsDirectorySeparator(c).Should().BeTrue();
    }

    [Theory]
    [InlineData('\\')]
    [InlineData('/')]
    public static void IsDirectorySeparator_WindowsUncRecognizesBoth(char c)
    {
        PathXOptions.WindowsUncPaths.IsDirectorySeparator(c).Should().BeTrue();
    }

    [Theory]
    [InlineData('/')]
    public static void IsDirectorySeparator_UnixRecognizesSlash(char c)
    {
        PathXOptions.Unix.IsDirectorySeparator(c).Should().BeTrue();
    }

    [Theory]
    [InlineData('a')]
    [InlineData('b')]
    [InlineData(':')]
    public static void IsDirectorySeparator_WindowsShouldRejectNonSeparators(char c)
    {
        PathXOptions.Windows.IsDirectorySeparator(c).Should().BeFalse();
    }

    [Theory]
    [InlineData('a')]
    [InlineData('b')]
    [InlineData(':')]
    [InlineData('\\')]
    public static void IsDirectorySeparator_UnixShouldRejectNonSeparators(char c)
    {
        PathXOptions.Unix.IsDirectorySeparator(c).Should().BeFalse();
    }

    [Theory]
    [InlineData(true)] // default ImmutableArray<char>
    [InlineData(false)] // empty ImmutableArray<char>
    public static void Ctor_ShouldThrowEmptyCollectionException_WhenRecognizedSeparatorsIsDefaultOrEmpty(
        bool useDefault
    )
    {
        var recognizedSeparators = useDefault ? default : ImmutableArray<char>.Empty;

        // ReSharper disable once ObjectCreationAsStatement
        // ReSharper disable once HeapView.ObjectAllocation.Evident
#pragma warning disable CA1806
        Action act = () => new PathXOptions(
            recognizedSeparators: recognizedSeparators,
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
#pragma warning restore CA1806

        act.Should().Throw<EmptyCollectionException>().WithParameterName("recognizedSeparators");
    }

    [Fact]
    public static void Ctor_ShouldThrowMissingItemException_WhenPreferredSeparatorNotContained()
    {
        var recognizedSeparators = ImmutableArray.Create('\\');

        // ReSharper disable once ObjectCreationAsStatement
        // ReSharper disable once HeapView.ObjectAllocation.Evident
#pragma warning disable CA1806
        Action act = () => new PathXOptions(
            recognizedSeparators: recognizedSeparators,
            preferredSeparator: '/', // not in recognizedSeparators
            volumeSeparator: null,
            isSupportingDriveLetters: false,
            isSupportingUncPaths: false,
            isSupportingDevicePaths: false,
            isCaseSensitive: true,
            collapseSeparatorRuns: true,
            preserveSeparatorsAfterRoot: true,
            trimTrailingSeparatorsExceptRoot: true
        );
#pragma warning restore CA1806

        act.Should().Throw<MissingItemException>().WithParameterName("recognizedSeparators");
    }
}
