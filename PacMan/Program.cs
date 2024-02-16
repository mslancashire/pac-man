﻿// Ignore Spelling: Pacs

using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Internal;
using System.ComponentModel;
using NUnit.Framework.Internal.Execution;

/**
 * Grab the pellets as fast as you can!
 **/
class Player
{
    static void Main(string[] args)
    {
        var game = GameFactory.BuildGame();
        game.ReadBoard();

        // game loop
        while (true)
        {
            game.ReadTurnInfo();
            game.ReadCurrentPacState(game.CurrentTurn);
            game.ReadVisiblePellets();

            var commands = game.IssueCommands();
            var commandsAsIOSyntax = game.CommandsAsIOSyntax(commands);

            Console.Error.WriteLine(commandsAsIOSyntax);
            Console.WriteLine(commandsAsIOSyntax);
        }
    }
}

public static class GameFactory
{
    public static Game BuildGame()
        => BuildGame(new GameConsoleIO());

    public static Game BuildGame(IGameIO gameIO)
        => Game.CreateGame(gameIO);
}

public class Game
{
    public bool IsDebug = false;
    private IGameIO _gameIO;

    public static Game CreateGame(IGameIO gameIO)
        => new(gameIO);

    private Game(IGameIO gameIO)
    {
        _gameIO = gameIO;
    }

    public void SetGameIO(IGameIO gameIO)
        => _gameIO = gameIO;

    public Board Board = Empties.CreateEmptyBoard();
    public TurnInfo CurrentTurn = Empties.CreateEmptyTurnInfo();
    public Dictionary<int, Pac> CurrentPacs = new();
    public List<Pellet> VisiblePellets = new();

    /// <summary>
    /// Reads the Board from Game IO.
    /// Top left corner is (x=0, y=0).
    /// One line of the grid: space " " is floor, pound "#" is wall.
    /// </summary>
    public void ReadBoard()
    {
        var inputs = _gameIO.ReadInput().Split(' ');
        var board = new Board(int.Parse(inputs[0]), int.Parse(inputs[1]));

        // build the board        
        for (int i = 0; i < board.Height; i++)
        {
            board.Rows.Add(_gameIO.ReadInput());
        }

        Board = board;

        Log(board);
    }

    /// <summary>
    /// Reads the Turn Info from Game IO.
    /// Gets the scores and the number of visible Pacs.
    /// </summary>
    public void ReadTurnInfo()
    {
        var scores = _gameIO.ReadInput().Split(' ');
        int visiblePacCount = int.Parse(_gameIO.ReadInput()); // all your pacs and enemy pacs in sight

        var turnInfo = new TurnInfo(int.Parse(scores[0]), int.Parse(scores[1]), visiblePacCount);

        CurrentTurn = turnInfo;

        Log(turnInfo);
    }

    /// <summary>
    /// Reads the Current Pac states at the start of a Turn.
    /// </summary>
    /// <param name="turnInfo"></param>
    public void ReadCurrentPacState(TurnInfo turnInfo)
    {
        for (int i = 0; i < turnInfo.VisiblePacCount; i++)
        {
            var inputs = _gameIO.ReadInput().Split(' ');
            
            var id = int.Parse(inputs[0]); // pac number (unique within a team)
            var isMine = inputs[1] != "0"; // true if this pac is yours
            var gridX = int.Parse(inputs[2]); // position in the grid
            var gridY = int.Parse(inputs[3]); // position in the grid
            var typeId = inputs[4]; // unused in wood leagues
            var turnSpeed = int.Parse(inputs[5]); // unused in wood leagues
            var abilityCooldown = int.Parse(inputs[6]); // unused in wood leagues

            if (CurrentPacs.TryGetValue(id, out var existingPac))
            {
                // TODO: Deal with current pac state
                CurrentPacs.Remove(id);
            }

            var newPac = new Pac(id, isMine, gridX, gridY, typeId, turnSpeed, abilityCooldown);

            CurrentPacs.Add(newPac.Id, newPac);
        }

        Log(CurrentPacs.Values);
    }

    /// <summary>
    /// Reads the visible Pellets from Game IO.
    /// `{GridX} {GridY} {PelletValue}`
    /// Normal Pellets = 1.
    /// Large Pellets = 10.
    /// </summary>
    /// <example>12 1 10</example>
    public void ReadVisiblePellets()
    {
        VisiblePellets.Clear();

        int visiblePelletCount = int.Parse(_gameIO.ReadInput()); // all pellets in sight

        for (int i = 0; i < visiblePelletCount; i++)
        {
            var inputs = _gameIO.ReadInput().Split(' ');

            var pellet = new Pellet(int.Parse(inputs[0]), int.Parse(inputs[1]), int.Parse(inputs[2]));

            VisiblePellets.Add(pellet);
        }

        Log(VisiblePellets);
    }

    public IEnumerable<ICommand> IssueCommands()
    {
        var pacCommands = new List<ICommand>();
        var viablePellets = VisiblePellets;

        foreach (var myPac in MyPacs)
        {
            var nearestPellet = viablePellets
                .OrderByDescending(vp => vp.Value)
                .ThenBy(myPac.MovesTo)
                .First();

            pacCommands.Add(new MoveCommand(myPac.Id, nearestPellet.GridX, nearestPellet.GridY));

            viablePellets.Remove(nearestPellet);
        }

        return pacCommands;
    }

    public string CommandsAsIOSyntax(IEnumerable<ICommand> commands)
    {
        var commandsAsIOSyntax = string.Join(" | ", commands.Select(c => c.ToCommandSyntax()));
        commandsAsIOSyntax = commandsAsIOSyntax.TrimEnd(' ', '|').TrimEnd(' ', '|');

        return commandsAsIOSyntax;
    }

    public IEnumerable<Pac> MyPacs => SearchPacs(p => p.IsMine);

    public IEnumerable<Pac> EnemyPacs => SearchPacs(p => p.IsMine == false);

    public IEnumerable<Pac> SearchPacs(Func<Pac,bool> find) => CurrentPacs.Values.Where(find);

    public bool TryGetPac(int id, out Pac pac)
        => CurrentPacs.TryGetValue(id, out pac!);

    private void Log(IEnumerable<IPrintable> items)
    {
        if (IsDebug == false)
        {
            return;
        }

        items.ToList().ForEach(Log);
    }

    private void Log(IPrintable item)
    {
        if (IsDebug) item.Print(_gameIO);
    }
}

public record Board(int Width, int Height) : IPrintable
{
    private List<string> _rows = new();

    public List<string> Rows { get => _rows; }

    public void Print(IGameIO gameIO)
    {
        gameIO.LogInfo($"Board is {Width} by {Height}.");
        _rows.ForEach(r => gameIO.LogInfo(r));
    }
}

public record TurnInfo(int MyScore, int OpponentsScore, int VisiblePacCount) : IPrintable
{
    public void Print(IGameIO gameIO)
    {
        gameIO.LogInfo(ToString());
    }
}

public record Pac(int Id, bool IsMine, int GridX, int GridY, string TypeId, int TurnSpeed, int AbilityCoolDown)
    : IPrintable, ILocatable
{
    public int MovesTo(ILocatable locationToMoveTo)
    {
        return Math.Abs(GridX - locationToMoveTo.GridX) + Math.Abs(GridY - locationToMoveTo.GridY);
    }

    public void Print(IGameIO gameIO)
    {
        gameIO.LogInfo(ToString());
    }
}

public record Pellet(int GridX, int GridY, int Value) : IPrintable, ILocatable
{
    public void Print(IGameIO gameIO)
    {
        gameIO.LogInfo($"Pellet of value {Value} is located at {GridX} by {GridY}.");
    }
}

public abstract record Command(string CommandType, int PacId) : ICommand
{
    public abstract string ToCommandSyntax();
}

public record MoveCommand(int PacId, int GridX, int GridY) : Command("Move", PacId), ILocatable
{
    public override string ToCommandSyntax()
    {
        return $"MOVE {PacId} {GridX} {GridY}";
    }
}

public interface IPrintable
{
    void Print(IGameIO gameIO);
}

public interface IGameIO
{
    string ReadInput();

    void IssueInstruction(string instruction);

    void LogInfo(string message);
}

public interface ILocatable
{
    int GridX { get; }

    int GridY { get; }
}

public interface ICommand
{
    int PacId { get; }
    
    string CommandType { get; }

    string ToCommandSyntax();
}

public class GameConsoleIO : IGameIO
{    
    public void IssueInstruction(string instruction)
        => Console.WriteLine(instruction);

    public void LogInfo(string message)
     => Console.Error.WriteLine(message);

    public string ReadInput()
        => Console.ReadLine()!;
}

public static class Empties
{
    public static Board CreateEmptyBoard()
        => new(0, 0);

    public static TurnInfo CreateEmptyTurnInfo()
        => new(0, 0, 0);

    public static Pac CreateEmptyPac()
        => new(0, true, 0, 0, "", 0, 0);

    public static Pellet CreateEmptyPellet()
        => new(0, 0, 0);
}