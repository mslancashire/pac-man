using FluentAssertions;
using PacMan.Tests.Helpers;
using Xunit.Abstractions;

namespace PacMan.Tests;

public class LogTests : BaseTestForGame
{
    public LogTests(ITestOutputHelper logger) : base(logger)
    {
    }
    
    [Theory]
    [InlineData(LogType.Setup, LogType.Setup)]
    [InlineData(LogType.Setup | LogType.Board, LogType.Setup)]
    [InlineData(LogType.Setup, LogType.Setup | LogType.Board)]
    public void Check_enum_flag_logic_for_log(LogType enabledLogs, LogType itemLogType)
    {
        // arrange

        // act
        var loggingIsEnabled = (enabledLogs & itemLogType) == enabledLogs
                               || (itemLogType & enabledLogs) == itemLogType;

        var loggingIsEnabledv1 = (enabledLogs & itemLogType) != 0;

        // assert
        loggingIsEnabled.Should().BeTrue();
        loggingIsEnabledv1.Should().BeTrue();
    }
}
