using FluentAssertions;
using PacMan.Tests.Helpers;
using Xunit.Abstractions;

namespace PacMan.Tests;

public class PelletTests : BaseTestForGame
{
    public PelletTests(ITestOutputHelper logger) : base(logger)
    {
    }

    [Fact]
    public void ReadVisiblePellets_should_read_game_input_and_produce_the_currently_visible_pellets_given_only_one()
    {
        // arrange
        var input = @"1
12 2 10";

        SetInput(input);

        // act
        Game.ReadVisiblePellets();

        // assert
        Game.VisiblePellets.Should().NotBeNullOrEmpty();
        Game.VisiblePellets.Should().HaveCount(1);
        Game.VisiblePellets.Should().HaveElementAt(0, new Pellet(12, 2, 10));
    }

    [Fact]
    public void ReadVisiblePellets_should_read_game_input_and_produce_the_currently_visible_pellets_given_multiple()
    {
        // arrange
        var input = @"5
12 2 10
0 0 1
5 5 10
14 6 1
2 12 1";

        SetInput(input);

        // act
        Game.ReadVisiblePellets();

        // arrange
        Game.VisiblePellets.Should().NotBeNullOrEmpty();
        Game.VisiblePellets.Should().HaveCount(5);
        Game.VisiblePellets.Should().HaveElementAt(0, new Pellet(12, 2, 10));
        Game.VisiblePellets.Should().HaveElementAt(1, new Pellet(0, 0, 1));
        Game.VisiblePellets.Should().HaveElementAt(2, new Pellet(5, 5, 10));
        Game.VisiblePellets.Should().HaveElementAt(3, new Pellet(14, 6, 1));
        Game.VisiblePellets.Should().HaveElementAt(4, new Pellet(2, 12, 1));
    }
}
