using Whycespace.Shared.Contracts.Events.Business.Agreement.PartyGovernance.Counterparty;
using DomainEvents = Whycespace.Domain.BusinessSystem.Agreement.PartyGovernance.Counterparty;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/agreement/party-governance/counterparty domain.
///
/// Owns the binding from Counterparty domain event CLR types to the
/// <see cref="EventSchemaRegistry"/>, plus the outbound payload mappers that
/// project domain events (with strongly-typed CounterpartyId) into the shared
/// schema records (Guid AggregateId) consumed by the projection layer.
/// </summary>
public sealed class BusinessAgreementPartyGovernanceCounterpartySchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "CounterpartyCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.CounterpartyCreatedEvent),
            typeof(CounterpartyCreatedEventSchema));

        sink.RegisterSchema(
            "CounterpartySuspendedEvent",
            EventVersion.Default,
            typeof(DomainEvents.CounterpartySuspendedEvent),
            typeof(CounterpartySuspendedEventSchema));

        sink.RegisterSchema(
            "CounterpartyTerminatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.CounterpartyTerminatedEvent),
            typeof(CounterpartyTerminatedEventSchema));

        sink.RegisterPayloadMapper("CounterpartyCreatedEvent", e =>
        {
            var evt = (DomainEvents.CounterpartyCreatedEvent)e;
            return new CounterpartyCreatedEventSchema(evt.CounterpartyId.Value);
        });
        sink.RegisterPayloadMapper("CounterpartySuspendedEvent", e =>
        {
            var evt = (DomainEvents.CounterpartySuspendedEvent)e;
            return new CounterpartySuspendedEventSchema(evt.CounterpartyId.Value);
        });
        sink.RegisterPayloadMapper("CounterpartyTerminatedEvent", e =>
        {
            var evt = (DomainEvents.CounterpartyTerminatedEvent)e;
            return new CounterpartyTerminatedEventSchema(evt.CounterpartyId.Value);
        });
    }
}
