using System.Text;
using Xunit.Abstractions;

namespace PacMan.Tests;

internal class GameTestIO : IGameIO
{
    private StringReader _reader;
    private readonly StringBuilder _writer;
    private readonly ITestOutputHelper _logger;
    
    internal GameTestIO(ITestOutputHelper logger)
    {
        _writer = new StringBuilder();
        _logger = logger;
    }

    public void SetInput(string input)
        => _reader = new StringReader(input);

    public void IssueInstruction(string instruction)
    {
        _writer.AppendLine(instruction);
    }

    public string ReadInput() => _reader.ReadLine();

    public string GetAllIssuedInstructions()
        => _writer.ToString();

    public void LogInfo(string message)
        => _logger.WriteLine(message);
}
