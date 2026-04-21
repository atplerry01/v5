using Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Customer;
using Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Customer;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Customer.IdentityAndProfile.Customer;

public sealed class RenameCustomerHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RenameCustomerCommand cmd)
            return;

        var aggregate = (CustomerAggregate)await context.LoadAggregateAsync(typeof(CustomerAggregate));
        aggregate.Rename(new CustomerName(cmd.Name));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
