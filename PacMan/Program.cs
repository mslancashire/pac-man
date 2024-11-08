﻿// Ignore Spelling: Pacs

using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

/**
 * Grab the pellets as fast as you can!
 **/
class Player
{
    static void Main(string[] args)
    {
        var game = GameFactory.BuildGame();
        game.ReadBoard();

        int turn = 0;

        // game loop
        while (true)
        {
            turn++;
            game.ReadTurnInfo();
            game.ReadCurrentPacState(game.CurrentTurn);
            game.ReadVisiblePellets();

            game.LogTurn(turn);

            var commands = game.IssueCommands(turn);
            var commandsAsIOSyntax = game.CommandsAsIOSyntax(commands);

            //Console.Error.WriteLine($"Issuing {commands.Count()} commands...");
            //Console.Error.WriteLine(commandsAsIOSyntax);

            Console.WriteLine(commandsAsIOSyntax);
        }
    }
}

public static class GameFactory
{
    public static Game BuildGame()
        => BuildGame(new GameConsoleIO(LogType.All));

    public static Game BuildGame(IGameIO gameIO)
        => Game.CreateGame(gameIO);
}

public class Game
{
    public bool IsDebug = true;
    public LogType EnabledLogs = LogType.All;

    private IGameIO _gameIO;

    public static Game CreateGame(IGameIO gameIO)
        => new(gameIO);

    private Game(IGameIO gameIO)
    {
        _gameIO = gameIO;
    }

    public void SetGameIO(IGameIO gameIO)
        => _gameIO = gameIO;

    public IGameIO IO => _gameIO;
    public Board Board = Empties.CreateEmptyBoard();
    public TurnInfo CurrentTurn = Empties.CreateEmptyTurnInfo();
    public Dictionary<(int, bool), Pac> CurrentPacs = new();
    public List<Pellet> VisiblePellets = new();

    /// <summary>
    /// Reads the Board from Game IO.
    /// Top left corner is (x=0, y=0).
    /// One line of the grid: space " " is floor, pound "#" is wall.
    /// </summary>
    public void ReadBoard()
    {
        var inputs = IO.ReadInput().Split(' ');
        var board = new Board(int.Parse(inputs[0]), int.Parse(inputs[1]));

        // build the board
        for (int i = 0; i < board.Height; i++)
        {
            board.Rows.Add(IO.ReadInput());
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
        var scores = IO.ReadInput().Split(' ');
        int visiblePacCount = int.Parse(IO.ReadInput()); // all your pacs and enemy pacs in sight

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
        // TODO: Deal with clashing Pacs.

        for (int i = 0; i < turnInfo.VisiblePacCount; i++)
        {
            var inputs = IO.ReadInput().Split(' ');

            var id = int.Parse(inputs[0]); // pac number (unique within a team)
            var isMine = inputs[1] != "0"; // true if this pac is yours
            var gridX = int.Parse(inputs[2]); // position in the grid
            var gridY = int.Parse(inputs[3]); // position in the grid
            var typeId = Enum.Parse<AbilityType>(inputs[4]); // unused in wood leagues
            var turnSpeed = int.Parse(inputs[5]); // unused in wood leagues
            var abilityCooldown = int.Parse(inputs[6]); // unused in wood leagues

            if (CurrentPacs.TryGetValue((id, isMine), out var existingPac))
            {
                CurrentPacs[existingPac.AsKey] = existingPac with { TypeId = typeId, TurnSpeed = turnSpeed, AbilityCoolDown = abilityCooldown };
                continue;
            }

            var newPac = new Pac(id, isMine, gridX, gridY, typeId, turnSpeed, abilityCooldown);

            CurrentPacs.Add(newPac.AsKey, newPac);
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
        // TODO: Read Visible Pellets to Board, then use Board in Command Issuer.

        VisiblePellets.Clear();

        int visiblePelletCount = int.Parse(IO.ReadInput()); // all pellets in sight

        for (int i = 0; i < visiblePelletCount; i++)
        {
            var inputs = IO.ReadInput().Split(' ');

            var pellet = new Pellet(int.Parse(inputs[0]), int.Parse(inputs[1]), int.Parse(inputs[2]));

            VisiblePellets.Add(pellet);
        }

        //Log(VisiblePellets);
    }

    public IEnumerable<ICommand> IssueCommands(int turn)
    {
        var pacCommands = new List<ICommand>();

        var pacsWithOutCommands = MyPacs;

        //pacCommands.AddRange(IssueSpeedCommands(turn, pacsWithOutCommands));

        pacsWithOutCommands = pacsWithOutCommands.Where(p => pacCommands.All(pc => pc.PacId != p.Id));

        // calculate commands for closest super pellets
        var viableSuperPellets = VisiblePellets.Where(p => p.Value > 1);
        foreach (var supperPellet in viableSuperPellets)
        {
            if (pacsWithOutCommands.Any() == false)
            {
                break;
            }

            var commandForPellet = IssueCommandForPellet(supperPellet, pacsWithOutCommands);
            if (commandForPellet == null)
            {
                continue;
            }

            pacCommands.Add(commandForPellet);
            pacsWithOutCommands = pacsWithOutCommands.Where(p => pacCommands.All(pc => pc.PacId != p.Id));
        }

        // calculate commands for closest normal pellets
        if (pacsWithOutCommands.Any())
        {
            var closestPellets = new List<Pellet>(VisiblePellets.Where(p => p.Value == 1));
            foreach (var pac in pacsWithOutCommands)
            {
                var closestPellet = closestPellets
                    .OrderBy(pac.MovesTo)
                    .FirstOrDefault();

                if (closestPellet is null)
                {
                    continue;
                }

                pacCommands.Add(new MoveCommand(pac, closestPellet));
                closestPellets.Remove(closestPellet);
            }
        }

        return pacCommands.OrderBy(c => c.PacId);
    }

    public ICommand? IssueCommandForPellet(Pellet pellet, IEnumerable<Pac> availablePacs)
    {
        if (availablePacs.Any() == false)
        {
            return null;
        }

        var closestPac = availablePacs
            .OrderBy(p => p.MovesTo(pellet))
            .First();

        return new MoveCommand(closestPac, pellet);
    }

    public List<ICommand> IssueSpeedCommands(int turn, IEnumerable<Pac> pacsWithOutCommands)
    {
        var pacCommands = new List<ICommand>();

        if (turn == 1)
        {
            foreach (var myPac in pacsWithOutCommands)
            {
                if (SpeedCommand.IsActionable(myPac) == false)
                {
                    continue;
                }

                pacCommands.Add(new SpeedCommand(myPac.Id));
            }
        }

        return pacCommands;
    }

    public string CommandsAsIOSyntax(IEnumerable<ICommand> commands)
    {
        var commandsAsIOSyntax = string.Join(" | ", commands.Select(c => c.ToCommandSyntax()));
        commandsAsIOSyntax = commandsAsIOSyntax.TrimEnd(' ', '|').TrimEnd(' ', '|');

        return commandsAsIOSyntax;
    }

    public IEnumerable<Pac> MyPacs => SearchPacs(p => p.IsMine && p.TypeId != AbilityType.DEAD);

    public IEnumerable<Pac> EnemyPacs => SearchPacs(p => p.IsMine == false);

    public IEnumerable<Pac> SearchPacs(Func<Pac, bool> find) => CurrentPacs.Values.Where(find);

    public bool TryGetPac((int, bool) key, out Pac pac)
        => CurrentPacs.TryGetValue(key, out pac!);

    public void LogTurn(int turnInfo)
    {
        if (turnInfo <= 1)
        {
            Log(CurrentTurn, LogType.Setup);
            Log(CurrentPacs.Values, LogType.Setup);
            Log(VisiblePellets, LogType.Setup);
            return;
        }

        Log(CurrentTurn);
        Log(CurrentPacs.Values);
        Log(VisiblePellets);
    }

    private void Log(IEnumerable<IPrintable> items)
        => Log(items, null);

    private void Log(IEnumerable<IPrintable> items, LogType? additionalLogType = null)
    {
        if (IsDebug == false)
        {
            return;
        }

        items.ToList().ForEach(i => Log(i, additionalLogType));
    }

    private void Log(IPrintable item)
        => Log(item, null);

    private void Log(IPrintable item, LogType? additionalLogType = null)
    {
        var loggableTypes = item.LogType;

        if (additionalLogType.HasValue)
        {
            loggableTypes = additionalLogType.Value | item.LogType;
        }

        if (IsDebug && (loggableTypes & EnabledLogs) != 0)
        {
            item.Print(IO);
        }
    }
}

public record Board(int Width, int Height) : IPrintable
{
    private List<string> _rows = new();

    public List<string> Rows { get => _rows; }

    public LogType LogType => LogType.Setup | LogType.Board;

    public void Print(IGameIO gameIO)
    {
        gameIO.LogInfo($"Board is {Width} by {Height}.", LogType);
        _rows.ForEach(r => gameIO.LogInfo(r, LogType));
    }
}

public record TurnInfo(int MyScore, int OpponentsScore, int VisiblePacCount) : IPrintable
{
    public LogType LogType => LogType.Turn;

    public void Print(IGameIO gameIO)
    {
        gameIO.LogInfo(ToString(), LogType);
    }
}

public record Pac(int Id, bool IsMine, int GridX, int GridY, AbilityType TypeId, int TurnSpeed, int AbilityCoolDown)
    : IPrintable, ILocatable
{
    public (int Id, bool IsMine) AsKey => (Id, IsMine);

    private ILocatable? _currentPosition = null;
    private bool _didNotMove = false;

    public void SetCurrentPosition(ILocatable location)
    {
        if (_currentPosition is null)
        {
            _currentPosition = location;
            return;
        }

        if (_currentPosition.GridX == location.GridX && _currentPosition.GridY == location.GridY)
        {
            _didNotMove = true;
        }

        _currentPosition = location;
    }

    public bool DidNotMove => _didNotMove;

    public void ResetMove()
        => _didNotMove = false;

    public int MovesTo(ILocatable locationToMoveTo)
    {
        return Math.Abs(GridX - locationToMoveTo.GridX) + Math.Abs(GridY - locationToMoveTo.GridY);
    }

    public LogType LogType => LogType.Pac;

    public void Print(IGameIO gameIO)
    {
        gameIO.LogInfo(ToString(), LogType);
    }
}

public record Pellet(int GridX, int GridY, int Value) : IPrintable, ILocatable
{
    public LogType LogType => LogType.Pellet;

    public void Print(IGameIO gameIO)
    {
        gameIO.LogInfo($"Pellet of value {Value} is located at {GridX} by {GridY}.", LogType);
    }
}

public abstract record Command(string CommandType, int PacId) : ICommand
{
    public LogType LogType => LogType.Command;

    public void Print(IGameIO gameIO)
    {
        gameIO.LogInfo(ToCommandSyntax(), LogType);
    }

    public abstract string ToCommandSyntax();
}

public record MoveCommand(int PacId, int GridX, int GridY) : Command("MOVE", PacId), ILocatable
{
    public MoveCommand(Pac pac, ILocatable location)
        : this(pac.Id, location.GridX, location.GridY)
    { }

    public override string ToCommandSyntax()
    {
        return $"{CommandType} {PacId} {GridX} {GridY}";
    }
}

public record SpeedCommand(int PacId) : Command("SPEED", PacId)
{
    internal static bool IsActionable(Pac myPac)
        => myPac.TurnSpeed == 0;

    public override string ToCommandSyntax()
    {
        return $"{CommandType} {PacId}";
    }
}

public record SwitchAbilityCommand(int PacId, AbilityType AbilityType) : Command("SWITCH", PacId)
{
    public override string ToCommandSyntax()
        => $"{CommandType} {PacId} {AbilityType}";
}

public record GridLocation(int GridX, int GridY) : ILocatable;

public interface IPrintable
{
    LogType LogType { get; }

    void Print(IGameIO gameIO);
}

public interface IGameIO
{
    string ReadInput();

    void IssueInstruction(string instruction);

    void LogInfo(string message, LogType logType);
}

public interface ILocatable
{
    int GridX { get; }

    int GridY { get; }
}

public interface ICommand : IPrintable
{
    int PacId { get; }

    string CommandType { get; }

    string ToCommandSyntax();
}

public class GameConsoleIO : IGameIO
{
    private readonly LogType _enabledLogs;

    public GameConsoleIO(LogType enabledLogs)
    {
        _enabledLogs = enabledLogs;
    }

    public void IssueInstruction(string instruction)
        => Console.WriteLine(instruction);

    public void LogInfo(string message, LogType logType)
    {
        if ((_enabledLogs & logType) == 0)
        {
            return;
        }

        Console.Error.WriteLine(message);
    }

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
        => new(0, true, 0, 0, AbilityType.NONE, 0, 0);

    public static Pellet CreateEmptyPellet()
        => new(0, 0, 0);
}

public enum AbilityType
{
    NONE,
    ROCK,
    SCISSORS,
    PAPER,
    DEAD
}

[Flags]
public enum LogType : int
{
    None = 0,
    Setup = 1,
    Board = 2,
    Turn = 4,
    Pac = 8,
    Pellet = 16,
    Command = 32,
    All = Setup | Board | Turn | Pac | Pellet | Command,
}