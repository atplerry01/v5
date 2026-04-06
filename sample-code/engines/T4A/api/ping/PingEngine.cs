using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T4A.Api.Ping;

public sealed record PingCommand(string AggregateId, string Message);

public sealed class PingEngine : IEngine<PingCommand>
{
    private readonly IClock _clock;

    public PingEngine(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public Task<EngineResult> ExecuteAsync(
        PingCommand command,
        EngineContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        return Task.FromResult(EngineResult.Ok(new
        {
            command.AggregateId,
            command.Message,
            Timestamp = _clock.UtcNowOffset
        }));
    }
}
