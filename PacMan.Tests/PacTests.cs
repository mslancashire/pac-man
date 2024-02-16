using FluentAssertions;
using PacMan.Tests.Helpers;
using Xunit.Abstractions;

namespace PacMan.Tests;

public class PacTests : BaseTestForGame
{
    public PacTests(ITestOutputHelper logger) : base(logger)
    {
    }

    [Fact]
    public void ReadCurrentPacState_should_read_console_lines_for_this_turns_pac_states_when_given_a_single_pac()
    {
        // arrange
        const int ID = 1;
        const bool IS_MINE = true;
        const int GRID_X = 13;
        const int GRID_Y = 2;
        const string TYPE_ID = "0";
        const int TURN_SPEED = 0;
        const int ABILITY_COOLDOWN = 0;

        var turnInfo = new TurnInfo(10, 5, 1);

        SetInput(SinglePacState);

        // act
        Game.ReadCurrentPacState(turnInfo);

        // assert
        Game.CurrentPacs.Should().NotBeNullOrEmpty();
        Game.CurrentPacs.Should().HaveCount(1);

        Game.TryGetPac((1, true), out var pac1).Should().BeTrue();
        pac1.ShouldBe(IS_MINE, GRID_X, GRID_Y);
    }

    [Fact]
    public void ReadCurrentPacState_should_read_console_lines_for_this_turns_pac_states_when_given_a_multiple_pacs()
    {
        // arrange
        var turnInfo = new TurnInfo(10, 5, 4);

        SetInput(MultiplePacStates);

        // act
        Game.ReadCurrentPacState(turnInfo);

        // assert
        Game.CurrentPacs.Should().NotBeNullOrEmpty();
        Game.CurrentPacs.Should().HaveCount(4);
        Game.MyPacs.Should().HaveCount(2);
        Game.EnemyPacs.Should().HaveCount(2);

        Game.TryGetPac((1, true), out var pac1).Should().BeTrue();
        Game.TryGetPac((2, false), out var pac2).Should().BeTrue();
        Game.TryGetPac((3, false), out var pac3).Should().BeTrue();
        Game.TryGetPac((4, true), out var pac4).Should().BeTrue();
        Game.TryGetPac((5, false), out _).Should().BeFalse();
        Game.TryGetPac((5, true), out _).Should().BeFalse();

        pac1.ShouldBe(true, 12, 2);
        pac2.ShouldBe(false, 2, 12);
        pac3.ShouldBe(false, 8, 8);
        pac4.ShouldBe(true, 10, 12);
    }

    [Theory]
    [InlineData(1, 1, 12, 5, 11 + 4)]
    [InlineData(12, 5, 1, 1, 11 + 4)]
    public void NumberOfMovesTo_should_return_correct_moves_to_a_location(int fromX, int fromY, int toX, int toY, int expectedMoves)
    {
        // arrange
        var pac = Empties.CreateEmptyPac() with { GridX = fromX, GridY = fromY };

        var pellet = Empties.CreateEmptyPellet() with { GridX = toX, GridY = toY };

        // act
        var numberOfMoves = pac.MovesTo(pellet);

        // assert
        numberOfMoves.Should().Be(expectedMoves);
    }

    [Fact]
    public void DidNotMove_should_be_false_on_first_init()
    {
        // arrange
        var pac = Empties.CreateEmptyPac() with { GridX = 5, GridY = 10 };

        // act
        pac.SetCurrentPosition(pac);

        // assert
        pac.DidNotMove.Should().BeFalse();
    }

    [Fact]
    public void SetCurrentPosition_should_set_did_not_move_to_true_when_location_is_the_same()
    {
        // arrange
        var pac = Empties.CreateEmptyPac() with { GridX = 5, GridY = 10 };
        pac.SetCurrentPosition(pac);

        var newPosition = new GridLocation(5, 10);

        // act
        pac.SetCurrentPosition(newPosition);

        // assert
        pac.DidNotMove.Should().BeTrue();
    }

    [Theory]
    [InlineData(6, 10)]
    [InlineData(5, 11)]
    public void SetCurrentPosition_should_set_did_not_move_to_false_when_location_is_different(int gridX, int gridY)
    {
        // arrange
        var pac = Empties.CreateEmptyPac() with { GridX = 5, GridY = 10 };
        pac.SetCurrentPosition(pac);

        var newPosition = new GridLocation(gridX, gridY);

        // act
        pac.SetCurrentPosition(newPosition);

        // assert
        pac.DidNotMove.Should().BeFalse();
    }



    private string SinglePacState =>
        "1 1 13 2 0 0 0";

    private string MultiplePacStates =>
        @"1 1 12 2 0 0 0
2 0 2 12 0 0 0
3 0 8 8 0 0 0
4 1 10 12 0 0 0";
}
