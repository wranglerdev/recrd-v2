using Microsoft.Extensions.Logging;
using Recrd.Application.Auditing;

namespace Recrd.Application.Tests;

public class AuditTrailTests
{
    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => null!;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) => Messages.Add(formatter(state, exception));
    }

    [Fact]
    public void Logs_every_important_event()
    {
        var logger = new RecordingLogger<AuditTrail>();
        var trail = new AuditTrail(logger);
        var id = Guid.NewGuid();

        trail.MassaImported("login", 3);
        trail.TestChanged(id);
        trail.Compiled(id, 1);
        trail.Exported(id, "login.robot");
        trail.Executed(id, "Passed");

        logger.Messages.Should().HaveCount(5);
        logger.Messages[0].Should().Contain("login").And.Contain("3");
        logger.Messages[4].Should().Contain("Passed");
    }
}
