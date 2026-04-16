using Whycespace.Domain.EconomicSystem.Subject.Subject;
using Whycespace.Domain.EconomicSystem.Vault.Account;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Shared.Contracts.Economic.Vault.Account;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Vault.Account;

public sealed class CreateVaultAccountHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateVaultAccountCommand cmd)
            return Task.CompletedTask;

        var aggregate = VaultAccountAggregate.Create(
            VaultAccountId.From(cmd.VaultAccountId),
            SubjectId.From(cmd.OwnerSubjectId),
            new Currency(cmd.Currency));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
