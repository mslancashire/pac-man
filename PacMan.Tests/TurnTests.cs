using FluentAssertions;
using PacMan.Tests.Helpers;
using Xunit.Abstractions;

namespace PacMan.Tests;

public class TurnTests : BaseTestForGame
{
    public TurnTests(ITestOutputHelper logger) : base(logger)
    {
    }

    [Fact]
    public void ReadTurnInfo_should_read_console_line_for_current_turn_info()
    {
        // arrange
        SetInput(@"10 5
10");

        // act
        Game.ReadTurnInfo();

        // assert
        Game.CurrentTurn.Should().NotBeNull();
        Game.CurrentTurn.MyScore.Should().Be(10);
        Game.CurrentTurn.OpponentsScore.Should().Be(5);
        Game.CurrentTurn.VisiblePacCount.Should().Be(10);
    }
}
