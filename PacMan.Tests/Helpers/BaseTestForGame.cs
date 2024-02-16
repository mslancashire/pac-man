using Xunit.Abstractions;

namespace PacMan.Tests.Helpers;

public abstract class BaseTestForGame
{
    protected readonly ITestOutputHelper Logger;
    private readonly GameTestIO GameTestIO;
    protected readonly Game Game;

    internal BaseTestForGame(ITestOutputHelper logger)
    {
        Logger = logger;
        GameTestIO = new GameTestIO(logger);
        Game = GameFactory.BuildGame(GameTestIO);
        Game.IsDebug = true;
    }

    protected void SetInput(string input)
        => GameTestIO.SetInput(input);
}
