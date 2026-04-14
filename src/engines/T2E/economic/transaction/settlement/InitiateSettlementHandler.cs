using Whycespace.Domain.EconomicSystem.Transaction.Settlement;
using Whycespace.Shared.Contracts.Economic.Transaction.Settlement;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Transaction.Settlement;

public sealed class InitiateSettlementHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not InitiateSettlementCommand cmd)
            return Task.CompletedTask;

        var aggregate = SettlementAggregate.Initiate(
            new SettlementId(cmd.SettlementId),
            SettlementAmount.From(cmd.Amount),
            SettlementCurrency.From(cmd.Currency),
            SettlementSourceReference.From(cmd.SourceReference),
            SettlementProvider.From(cmd.Provider));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
