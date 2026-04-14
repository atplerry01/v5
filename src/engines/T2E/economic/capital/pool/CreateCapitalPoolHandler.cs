using Whycespace.Domain.EconomicSystem.Capital.Pool;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Capital.Pool;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Capital.Pool;

public sealed class CreateCapitalPoolHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateCapitalPoolCommand cmd)
            return Task.CompletedTask;

        var aggregate = CapitalPoolAggregate.Create(
            new PoolId(cmd.PoolId),
            new Currency(cmd.Currency),
            new Timestamp(cmd.CreatedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
