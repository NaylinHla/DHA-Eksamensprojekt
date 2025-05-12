using Infrastructure.Logging;
using NUnit.Framework;

namespace Startup.Tests.MonitorTests;

[TestFixture]
public class MonitorServiceTests
{
    [Test]
    public void Log_ShouldNotBeNull()
    {
        // Act
        var logger = MonitorService.Log;
    
        // Assert
        Assert.That(logger, Is.Not.Null);
    }

    [Test]
    public void Log_ShouldLogInformationWithoutThrowing()
    {
        // Arrange
        var logger = MonitorService.Log;

        // Act & Assert
        Assert.DoesNotThrow(() => logger.Information("Test log from unit test"));
    }

    [Test]
    public void Log_ShouldLogToConsole()
    {
        // Arrange
        using var sw = new StringWriter();
        Console.SetOut(sw);

        // Act
        MonitorService.Log.Information("Console test log");

        // Flush and check output
        sw.Flush();
        var output = sw.ToString();

        // Assert
        Assert.That(output, Does.Contain("Console test log"));
    }
}