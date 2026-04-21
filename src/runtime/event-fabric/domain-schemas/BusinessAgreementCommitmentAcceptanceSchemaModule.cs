using Whycespace.Shared.Contracts.Events.Business.Agreement.Commitment.Acceptance;
using DomainEvents = Whycespace.Domain.BusinessSystem.Agreement.Commitment.Acceptance;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/agreement/commitment/acceptance domain.
/// </summary>
public sealed class BusinessAgreementCommitmentAcceptanceSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "AcceptanceCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.AcceptanceCreatedEvent),
            typeof(AcceptanceCreatedEventSchema));

        sink.RegisterSchema(
            "AcceptanceAcceptedEvent",
            EventVersion.Default,
            typeof(DomainEvents.AcceptanceAcceptedEvent),
            typeof(AcceptanceAcceptedEventSchema));

        sink.RegisterSchema(
            "AcceptanceRejectedEvent",
            EventVersion.Default,
            typeof(DomainEvents.AcceptanceRejectedEvent),
            typeof(AcceptanceRejectedEventSchema));

        sink.RegisterPayloadMapper("AcceptanceCreatedEvent", e =>
        {
            var evt = (DomainEvents.AcceptanceCreatedEvent)e;
            return new AcceptanceCreatedEventSchema(evt.AcceptanceId.Value);
        });
        sink.RegisterPayloadMapper("AcceptanceAcceptedEvent", e =>
        {
            var evt = (DomainEvents.AcceptanceAcceptedEvent)e;
            return new AcceptanceAcceptedEventSchema(evt.AcceptanceId.Value);
        });
        sink.RegisterPayloadMapper("AcceptanceRejectedEvent", e =>
        {
            var evt = (DomainEvents.AcceptanceRejectedEvent)e;
            return new AcceptanceRejectedEventSchema(evt.AcceptanceId.Value);
        });
    }
}
