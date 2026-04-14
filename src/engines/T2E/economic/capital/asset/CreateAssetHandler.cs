using Whycespace.Domain.EconomicSystem.Capital.Asset;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Capital.Asset;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Capital.Asset;

public sealed class CreateAssetHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateAssetCommand cmd)
            return Task.CompletedTask;

        var aggregate = AssetAggregate.Create(
            new AssetId(cmd.AssetId),
            cmd.OwnerId,
            new Amount(cmd.InitialValue),
            new Currency(cmd.Currency),
            new Timestamp(cmd.CreatedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
