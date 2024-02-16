using FluentAssertions;
using PacMan.Tests.Helpers;
using Xunit.Abstractions;

namespace PacMan.Tests;

public class CommandTests : BaseTestForGame
{
    public CommandTests(ITestOutputHelper logger) : base(logger)
    {
    }

    [Fact]
    public void MoveCommand_should_issue_correct_command_syntax()
    {
        // arrange
        var moveCommand = new MoveCommand(1, 15, 4);

        // act
        var moveSyntax = moveCommand.ToCommandSyntax();

        // assert
        moveSyntax.Should().Be("MOVE 1 15 4");
    }

    [Fact]
    public void CommandsAsIOSyntax_should_be_correct_based_on_a_single_move_command()
    {
        // arrange
        var commands = new[]
        {
            new MoveCommand(2, 0 , 8)
        };

        // act
        var commandSyntax = Game.CommandsAsIOSyntax(commands);

        // assert
        commandSyntax.Should().Be("MOVE 2 0 8");
    }

    [Fact]
    public void CommandsAsIOSyntax_should_be_correct_based_on_a_multiple_move_commands()
    {
        // arrange
        var commands = new[]
        {
            new MoveCommand(2, 0 , 8),
            new MoveCommand(3, 15 , 8),
            new MoveCommand(8, 4 , 6),
        };

        // act
        var commandSyntax = Game.CommandsAsIOSyntax(commands);

        // assert
        commandSyntax.Should().Be("MOVE 2 0 8 | MOVE 3 15 8 | MOVE 8 4 6");
    }
}
