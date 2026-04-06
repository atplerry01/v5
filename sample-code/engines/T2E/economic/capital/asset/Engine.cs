using Whycespace.Domain.EconomicSystem.Capital.Asset;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Capital.Asset;

public sealed class AssetEngine
{
    private readonly AssetPolicyAdapter _policy = new();

    public async Task<EngineResult> ExecuteAsync(AssetCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            CreateAssetCommand c => await CreateAsync(c, context),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private static async Task<EngineResult> CreateAsync(CreateAssetCommand command, EngineContext context)
    {
        var aggregate = await context.LoadAggregate<AssetAggregate>(command.Id);
        aggregate.Create(Guid.Parse(command.Id));
        await context.EmitEvents(aggregate);
        return EngineResult.Ok(context.EmittedEvents);
    }
}
