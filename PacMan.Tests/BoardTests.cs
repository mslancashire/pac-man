using FluentAssertions;
using PacMan.Tests.Helpers;
using Xunit.Abstractions;

namespace PacMan.Tests;

public class BoardTests : BaseTestForGame
{
    public BoardTests(ITestOutputHelper logger) : base(logger)
    {
    }

    [Fact]
    public void ReadBoard_should_read_console_lines_for_width_height_and_board()
    {
        // arrange
        var input = @"36 14
###################################
### ###   # # #     # # #   ### ###
### ### ### # # # # # # ### ### ###
###       #     # #     #       ###
### ### # # # ####### # # # ### ###
      #     #         #     #      
### # # ##### # # # # ##### # # ###
# #   #         # #         #   # #
# # # # # # # ####### # # # # # # #
# # #     #             #     # # #
# # ### ##### # # # # ##### ### # #
#         #     # #     #         #
##### ### # # ####### # # ### #####
###################################";

        SetInput(input);

        // act
        Game.ReadBoard();

        // assert
        Game.Board.Should().NotBeNull();
        Game.Board.Width.Should().Be(36);
        Game.Board.Height.Should().Be(14);
        Game.Board.Rows.Should().HaveCount(14);
        Game.Board.Rows.Should().AllSatisfy(r => r.Should().HaveLength(35));
    }
}
