using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Operational.Mobility;

public sealed class MobilityCommandHandler
{
    private readonly MobilityCreateEngine _createEngine = new();

    public Task<EngineResult> HandleAsync(
        MobilityCommand command,
        EngineContext context,
        CancellationToken ct) => command switch
    {
        CreateMobilityCommand create => _createEngine.ExecuteAsync(create, context, ct),
        _ => throw new System.NotSupportedException($"Unknown command: {command.GetType().Name}")
    };
}
