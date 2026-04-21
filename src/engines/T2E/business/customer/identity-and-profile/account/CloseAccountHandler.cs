using Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Account;
using Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Account;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Customer.IdentityAndProfile.Account;

public sealed class CloseAccountHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CloseAccountCommand)
            return;

        var aggregate = (AccountAggregate)await context.LoadAggregateAsync(typeof(AccountAggregate));
        aggregate.Close();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
