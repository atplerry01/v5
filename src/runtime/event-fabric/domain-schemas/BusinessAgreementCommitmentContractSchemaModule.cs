using Whycespace.Shared.Contracts.Events.Business.Agreement.Commitment.Contract;
using DomainEvents = Whycespace.Domain.BusinessSystem.Agreement.Commitment.Contract;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/agreement/commitment/contract domain.
///
/// Owns the binding from Contract domain event CLR types to the
/// <see cref="EventSchemaRegistry"/>, plus the outbound payload mappers that
/// project domain events (with strongly-typed ContractId) into the shared
/// schema records (Guid AggregateId) consumed by the projection layer.
/// </summary>
public sealed class BusinessAgreementCommitmentContractSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ContractCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ContractCreatedEvent),
            typeof(ContractCreatedEventSchema));

        sink.RegisterSchema(
            "ContractPartyAddedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ContractPartyAddedEvent),
            typeof(ContractPartyAddedEventSchema));

        sink.RegisterSchema(
            "ContractActivatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ContractActivatedEvent),
            typeof(ContractActivatedEventSchema));

        sink.RegisterSchema(
            "ContractSuspendedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ContractSuspendedEvent),
            typeof(ContractSuspendedEventSchema));

        sink.RegisterSchema(
            "ContractTerminatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ContractTerminatedEvent),
            typeof(ContractTerminatedEventSchema));

        sink.RegisterPayloadMapper("ContractCreatedEvent", e =>
        {
            var evt = (DomainEvents.ContractCreatedEvent)e;
            return new ContractCreatedEventSchema(evt.ContractId.Value, evt.CreatedAt);
        });
        sink.RegisterPayloadMapper("ContractPartyAddedEvent", e =>
        {
            var evt = (DomainEvents.ContractPartyAddedEvent)e;
            return new ContractPartyAddedEventSchema(evt.ContractId.Value, evt.PartyId.Value, evt.Role);
        });
        sink.RegisterPayloadMapper("ContractActivatedEvent", e =>
        {
            var evt = (DomainEvents.ContractActivatedEvent)e;
            return new ContractActivatedEventSchema(evt.ContractId.Value);
        });
        sink.RegisterPayloadMapper("ContractSuspendedEvent", e =>
        {
            var evt = (DomainEvents.ContractSuspendedEvent)e;
            return new ContractSuspendedEventSchema(evt.ContractId.Value);
        });
        sink.RegisterPayloadMapper("ContractTerminatedEvent", e =>
        {
            var evt = (DomainEvents.ContractTerminatedEvent)e;
            return new ContractTerminatedEventSchema(evt.ContractId.Value);
        });
    }
}
