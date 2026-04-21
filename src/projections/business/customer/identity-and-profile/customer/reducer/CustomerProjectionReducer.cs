using Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Customer;
using Whycespace.Shared.Contracts.Events.Business.Customer.IdentityAndProfile.Customer;

namespace Whycespace.Projections.Business.Customer.IdentityAndProfile.Customer.Reducer;

public static class CustomerProjectionReducer
{
    public static CustomerReadModel Apply(CustomerReadModel state, CustomerCreatedEventSchema e) =>
        state with
        {
            CustomerId = e.AggregateId,
            Name = e.Name,
            Type = e.Type,
            ReferenceCode = e.ReferenceCode,
            Status = "Draft"
        };

    public static CustomerReadModel Apply(CustomerReadModel state, CustomerRenamedEventSchema e) =>
        state with
        {
            CustomerId = e.AggregateId,
            Name = e.Name
        };

    public static CustomerReadModel Apply(CustomerReadModel state, CustomerReclassifiedEventSchema e) =>
        state with
        {
            CustomerId = e.AggregateId,
            Type = e.Type
        };

    public static CustomerReadModel Apply(CustomerReadModel state, CustomerActivatedEventSchema e) =>
        state with
        {
            CustomerId = e.AggregateId,
            Status = "Active"
        };

    public static CustomerReadModel Apply(CustomerReadModel state, CustomerArchivedEventSchema e) =>
        state with
        {
            CustomerId = e.AggregateId,
            Status = "Archived"
        };
}
