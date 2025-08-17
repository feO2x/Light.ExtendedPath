using System;
using FluentAssertions;
using Xunit;

namespace Light.ExtendedPath;

public static class PathXConstructorTests
{
    public static readonly TheoryData<PathXOptions> PathXOptionsValidCases =
        [PathXOptions.Windows, PathXOptions.Unix, PathXOptions.WindowsUncPaths];

    [Fact]
    public static void Constructor_ShouldThrow_WhenOptionsArgumentIsNull()
    {
        var act = () => new PathX(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Theory]
    [MemberData(nameof(PathXOptionsValidCases))]
    public static void Constructor_ShouldAssignOptions_WhenOptionsArgumentIsValid(PathXOptions options) =>
        new PathX(options).Options.Should().BeSameAs(options);
}
