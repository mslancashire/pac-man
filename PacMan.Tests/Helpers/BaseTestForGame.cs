using System.Reflection;
using Xunit.Abstractions;

namespace PacMan.Tests.Helpers;

public abstract class BaseTestForGame
{
    protected readonly ITestOutputHelper Logger;
    private readonly GameTestIO GameTestIO;
    protected readonly Game Game;
    private readonly string _pathForTests;

    internal BaseTestForGame(ITestOutputHelper logger)
    {
        Logger = logger;
        GameTestIO = new GameTestIO(logger);
        Game = GameFactory.BuildGame(GameTestIO);
        Game.IsDebug = true;

        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        _pathForTests = Path.Combine(path, "IntergrationTests");
    }

    protected void SetInput(string input)
        => GameTestIO.SetInput(input);

    protected void SetInputFromFile(string fileName)
    {
        var lines = File.ReadAllLines(Path.Combine(_pathForTests, fileName));
        GameTestIO.SetInput(string.Join("\r\n", lines));
    }
}
