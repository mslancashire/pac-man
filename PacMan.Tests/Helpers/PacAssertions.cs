using FluentAssertions;

namespace PacMan.Tests;

internal static class PacAssertions
{
    internal static void ShouldBe(this Pac pac, bool isMine, int gridX, int gridY)
    {
        pac.IsMine.Should().Be(isMine);
        pac.GridX.Should().Be(gridX);
        pac.GridY.Should().Be(gridY);
    }
}
