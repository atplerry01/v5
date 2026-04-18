using Whycespace.Domain.EconomicSystem.Transaction.Wallet;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Transaction.Wallet;

using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Transaction.Wallet;

public sealed class CreateWalletHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateWalletCommand cmd)
            return Task.CompletedTask;

        EnforcementGuard.RequireNotRestricted(context.EnforcementConstraint, context.IsSystem);

        var aggregate = WalletAggregate.Create(
            WalletId.From(cmd.WalletId),
            cmd.OwnerId,
            cmd.AccountId,
            new Timestamp(cmd.CreatedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
