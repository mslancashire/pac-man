using FluentAssertions;
using PacMan.Tests.Helpers;
using Xunit.Abstractions;

namespace PacMan.Tests;

public class GameIntegrationTests : BaseTestForGame
{
    public GameIntegrationTests(ITestOutputHelper logger) : base(logger)
    {
    }

    [Fact]
    public void GameSetup_should_be_correct_based_on_input()
    {
        // arrange
        SetInputFromFile("Board1.txt");

        // run and check board setup
        Game.ReadBoard();
        Game.Board.Width.Should().Be(35);
        Game.Board.Height.Should().Be(12);
        Game.Board.Rows.Should().HaveCount(12);
        Game.Board.Rows.Should().AllSatisfy(r => r.Should().HaveLength(35));

        // run and check turn one
        Game.ReadTurnInfo();
        
        // turn info
        Game.CurrentTurn.MyScore.Should().Be(0);
        Game.CurrentTurn.OpponentsScore.Should().Be(0);
        Game.CurrentTurn.VisiblePacCount.Should().Be(4);

        // pac state
        Game.ReadCurrentPacState(Game.CurrentTurn);
        Game.CurrentPacs.Count.Should().Be(4);
        Game.MyPacs.Count().Should().Be(3);
        Game.EnemyPacs.Count().Should().Be(1);

        // pellet state
        Game.ReadVisiblePellets();
        Game.VisiblePellets.Count().Should().Be(5);
    }
}
