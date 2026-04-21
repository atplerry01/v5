using Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Customer;
using Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Customer;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Customer.IdentityAndProfile.Customer;

public sealed class ReclassifyCustomerHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ReclassifyCustomerCommand cmd)
            return;

        var aggregate = (CustomerAggregate)await context.LoadAggregateAsync(typeof(CustomerAggregate));
        aggregate.Reclassify(Enum.Parse<CustomerType>(cmd.Type, ignoreCase: true));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
