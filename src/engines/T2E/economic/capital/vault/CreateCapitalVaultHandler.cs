using Whycespace.Domain.EconomicSystem.Capital.Vault;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Capital.Vault;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Capital.Vault;

public sealed class CreateCapitalVaultHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateCapitalVaultCommand cmd)
            return Task.CompletedTask;

        var aggregate = VaultAggregate.Create(
            new VaultId(cmd.VaultId),
            cmd.OwnerId,
            new Currency(cmd.Currency),
            new Timestamp(cmd.CreatedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
