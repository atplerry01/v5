using Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Customer;
using Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Customer;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Customer.IdentityAndProfile.Customer;

public sealed class CreateCustomerHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateCustomerCommand cmd)
            return Task.CompletedTask;

        CustomerReferenceCode? referenceCode = cmd.ReferenceCode is { Length: > 0 }
            ? new CustomerReferenceCode(cmd.ReferenceCode)
            : null;

        var aggregate = CustomerAggregate.Create(
            new CustomerId(cmd.CustomerId),
            new CustomerName(cmd.Name),
            Enum.Parse<CustomerType>(cmd.Type, ignoreCase: true),
            referenceCode);
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
