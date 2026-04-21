using Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Account;
using Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Account;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Customer.IdentityAndProfile.Account;

public sealed class CreateAccountHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateAccountCommand cmd)
            return Task.CompletedTask;

        var aggregate = AccountAggregate.Create(
            new AccountId(cmd.AccountId),
            new CustomerRef(cmd.CustomerId),
            new AccountName(cmd.Name),
            Enum.Parse<AccountType>(cmd.Type, ignoreCase: true));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
