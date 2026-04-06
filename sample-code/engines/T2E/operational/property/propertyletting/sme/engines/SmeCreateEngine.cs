using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Operational.Property.PropertyLetting.SME;

public sealed class SmeCreateEngine : IEngine<CreateSmeCommand>
{
    public Task<EngineResult> ExecuteAsync(
        CreateSmeCommand command,
        EngineContext context,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        return Task.FromResult(EngineResult.Ok(new { command.Id }));
    }
}
