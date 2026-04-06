using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Operational.Mobility;

public sealed class MobilityCreateEngine : IEngine<CreateMobilityCommand>
{
    public Task<EngineResult> ExecuteAsync(
        CreateMobilityCommand command,
        EngineContext context,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        return Task.FromResult(EngineResult.Ok(new { command.Id }));
    }
}
