using Whycespace.Domain.EconomicSystem.Capital.Reserve;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Capital.Reserve;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Capital.Reserve;

public sealed class CreateCapitalReserveHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateCapitalReserveCommand cmd)
            return Task.CompletedTask;

        var aggregate = ReserveAggregate.Create(
            new ReserveId(cmd.ReserveId),
            cmd.AccountId,
            new Amount(cmd.Amount),
            new Currency(cmd.Currency),
            new Timestamp(cmd.ReservedAt),
            new Timestamp(cmd.ExpiresAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
