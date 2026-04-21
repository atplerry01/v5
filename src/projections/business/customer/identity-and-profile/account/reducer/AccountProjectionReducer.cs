using Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Account;
using Whycespace.Shared.Contracts.Events.Business.Customer.IdentityAndProfile.Account;

namespace Whycespace.Projections.Business.Customer.IdentityAndProfile.Account.Reducer;

public static class AccountProjectionReducer
{
    public static AccountReadModel Apply(AccountReadModel state, AccountCreatedEventSchema e) =>
        state with
        {
            AccountId = e.AggregateId,
            CustomerId = e.CustomerId,
            Name = e.Name,
            Type = e.Type,
            Status = "Draft"
        };

    public static AccountReadModel Apply(AccountReadModel state, AccountRenamedEventSchema e) =>
        state with
        {
            AccountId = e.AggregateId,
            Name = e.Name
        };

    public static AccountReadModel Apply(AccountReadModel state, AccountActivatedEventSchema e) =>
        state with
        {
            AccountId = e.AggregateId,
            Status = "Active"
        };

    public static AccountReadModel Apply(AccountReadModel state, AccountSuspendedEventSchema e) =>
        state with
        {
            AccountId = e.AggregateId,
            Status = "Suspended"
        };

    public static AccountReadModel Apply(AccountReadModel state, AccountClosedEventSchema e) =>
        state with
        {
            AccountId = e.AggregateId,
            Status = "Closed"
        };
}
