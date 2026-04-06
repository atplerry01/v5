using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Operational.Property.PropertyLetting.SME;

public sealed class SmeCommandHandler
{
    private readonly SmeCreateEngine _createEngine = new();

    public Task<EngineResult> HandleAsync(
        SmeCommand command,
        EngineContext context,
        CancellationToken ct) => command switch
    {
        CreateSmeCommand create => _createEngine.ExecuteAsync(create, context, ct),
        _ => throw new System.NotSupportedException($"Unknown command: {command.GetType().Name}")
    };
}
