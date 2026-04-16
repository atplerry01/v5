using Whycespace.Domain.EconomicSystem.Transaction.Limit;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Transaction.Limit;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Transaction.Limit;

public sealed class DefineLimitHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DefineLimitCommand cmd)
            return Task.CompletedTask;

        if (!Enum.TryParse<LimitType>(cmd.Type, ignoreCase: true, out var limitType))
            throw new ArgumentException($"Unknown limit type '{cmd.Type}'.", nameof(cmd.Type));

        var aggregate = LimitAggregate.Define(
            LimitId.From(cmd.LimitId),
            cmd.AccountId,
            limitType,
            new Amount(cmd.Threshold),
            new Currency(cmd.Currency),
            new Timestamp(cmd.DefinedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
