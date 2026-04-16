using Whycespace.Domain.EconomicSystem.Transaction.Wallet;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Transaction.Wallet;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Transaction.Wallet;

public sealed class RequestWalletTransactionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RequestWalletTransactionCommand cmd)
            return;

        var aggregate = (WalletAggregate)await context.LoadAggregateAsync(typeof(WalletAggregate));

        aggregate.RequestTransaction(
            cmd.DestinationAccountId,
            new Amount(cmd.Amount),
            new Currency(cmd.Currency),
            new Timestamp(cmd.RequestedAt));

        context.EmitEvents(aggregate.DomainEvents);
    }
}
