using Whycespace.Shared.Contracts.Events.Business.Customer.IdentityAndProfile.Account;
using DomainEvents = Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Account;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/customer/identity-and-profile/account domain.
///
/// Owns the binding from Account domain event CLR types to the
/// <see cref="EventSchemaRegistry"/>, plus the outbound payload mappers that
/// project domain events (with strongly-typed AccountId, CustomerRef, AccountName,
/// AccountType) into the shared schema records (Guid AggregateId + primitives)
/// consumed by the projection layer.
/// </summary>
public sealed class BusinessCustomerIdentityAndProfileAccountSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "AccountCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.AccountCreatedEvent),
            typeof(AccountCreatedEventSchema));

        sink.RegisterSchema(
            "AccountRenamedEvent",
            EventVersion.Default,
            typeof(DomainEvents.AccountRenamedEvent),
            typeof(AccountRenamedEventSchema));

        sink.RegisterSchema(
            "AccountActivatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.AccountActivatedEvent),
            typeof(AccountActivatedEventSchema));

        sink.RegisterSchema(
            "AccountSuspendedEvent",
            EventVersion.Default,
            typeof(DomainEvents.AccountSuspendedEvent),
            typeof(AccountSuspendedEventSchema));

        sink.RegisterSchema(
            "AccountClosedEvent",
            EventVersion.Default,
            typeof(DomainEvents.AccountClosedEvent),
            typeof(AccountClosedEventSchema));

        sink.RegisterPayloadMapper("AccountCreatedEvent", e =>
        {
            var evt = (DomainEvents.AccountCreatedEvent)e;
            return new AccountCreatedEventSchema(
                evt.AccountId.Value,
                evt.Customer.Value,
                evt.Name.Value,
                evt.Type.ToString());
        });
        sink.RegisterPayloadMapper("AccountRenamedEvent", e =>
        {
            var evt = (DomainEvents.AccountRenamedEvent)e;
            return new AccountRenamedEventSchema(evt.AccountId.Value, evt.Name.Value);
        });
        sink.RegisterPayloadMapper("AccountActivatedEvent", e =>
        {
            var evt = (DomainEvents.AccountActivatedEvent)e;
            return new AccountActivatedEventSchema(evt.AccountId.Value);
        });
        sink.RegisterPayloadMapper("AccountSuspendedEvent", e =>
        {
            var evt = (DomainEvents.AccountSuspendedEvent)e;
            return new AccountSuspendedEventSchema(evt.AccountId.Value);
        });
        sink.RegisterPayloadMapper("AccountClosedEvent", e =>
        {
            var evt = (DomainEvents.AccountClosedEvent)e;
            return new AccountClosedEventSchema(evt.AccountId.Value);
        });
    }
}
