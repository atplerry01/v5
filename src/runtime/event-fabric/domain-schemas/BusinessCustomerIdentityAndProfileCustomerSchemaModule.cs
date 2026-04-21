using Whycespace.Shared.Contracts.Events.Business.Customer.IdentityAndProfile.Customer;
using DomainEvents = Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Customer;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/customer/identity-and-profile/customer domain.
///
/// Owns the binding from Customer domain event CLR types to the
/// <see cref="EventSchemaRegistry"/>, plus the outbound payload mappers that
/// project domain events (with strongly-typed CustomerId, CustomerName,
/// CustomerType, CustomerReferenceCode) into the shared schema records
/// (Guid AggregateId + primitives) consumed by the projection layer.
/// </summary>
public sealed class BusinessCustomerIdentityAndProfileCustomerSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "CustomerCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.CustomerCreatedEvent),
            typeof(CustomerCreatedEventSchema));

        sink.RegisterSchema(
            "CustomerRenamedEvent",
            EventVersion.Default,
            typeof(DomainEvents.CustomerRenamedEvent),
            typeof(CustomerRenamedEventSchema));

        sink.RegisterSchema(
            "CustomerReclassifiedEvent",
            EventVersion.Default,
            typeof(DomainEvents.CustomerReclassifiedEvent),
            typeof(CustomerReclassifiedEventSchema));

        sink.RegisterSchema(
            "CustomerActivatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.CustomerActivatedEvent),
            typeof(CustomerActivatedEventSchema));

        sink.RegisterSchema(
            "CustomerArchivedEvent",
            EventVersion.Default,
            typeof(DomainEvents.CustomerArchivedEvent),
            typeof(CustomerArchivedEventSchema));

        sink.RegisterPayloadMapper("CustomerCreatedEvent", e =>
        {
            var evt = (DomainEvents.CustomerCreatedEvent)e;
            return new CustomerCreatedEventSchema(
                evt.CustomerId.Value,
                evt.Name.Value,
                evt.Type.ToString(),
                evt.ReferenceCode?.Value);
        });
        sink.RegisterPayloadMapper("CustomerRenamedEvent", e =>
        {
            var evt = (DomainEvents.CustomerRenamedEvent)e;
            return new CustomerRenamedEventSchema(evt.CustomerId.Value, evt.Name.Value);
        });
        sink.RegisterPayloadMapper("CustomerReclassifiedEvent", e =>
        {
            var evt = (DomainEvents.CustomerReclassifiedEvent)e;
            return new CustomerReclassifiedEventSchema(evt.CustomerId.Value, evt.Type.ToString());
        });
        sink.RegisterPayloadMapper("CustomerActivatedEvent", e =>
        {
            var evt = (DomainEvents.CustomerActivatedEvent)e;
            return new CustomerActivatedEventSchema(evt.CustomerId.Value);
        });
        sink.RegisterPayloadMapper("CustomerArchivedEvent", e =>
        {
            var evt = (DomainEvents.CustomerArchivedEvent)e;
            return new CustomerArchivedEventSchema(evt.CustomerId.Value);
        });
    }
}
