using Whycespace.Shared.Contracts.Events.Business.Agreement.Commitment.Obligation;
using DomainEvents = Whycespace.Domain.BusinessSystem.Agreement.Commitment.Obligation;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/agreement/commitment/obligation domain.
/// </summary>
public sealed class BusinessAgreementCommitmentObligationSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ObligationCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ObligationCreatedEvent),
            typeof(ObligationCreatedEventSchema));

        sink.RegisterSchema(
            "ObligationFulfilledEvent",
            EventVersion.Default,
            typeof(DomainEvents.ObligationFulfilledEvent),
            typeof(ObligationFulfilledEventSchema));

        sink.RegisterSchema(
            "ObligationBreachedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ObligationBreachedEvent),
            typeof(ObligationBreachedEventSchema));

        sink.RegisterPayloadMapper("ObligationCreatedEvent", e =>
        {
            var evt = (DomainEvents.ObligationCreatedEvent)e;
            return new ObligationCreatedEventSchema(evt.ObligationId.Value);
        });
        sink.RegisterPayloadMapper("ObligationFulfilledEvent", e =>
        {
            var evt = (DomainEvents.ObligationFulfilledEvent)e;
            return new ObligationFulfilledEventSchema(evt.ObligationId.Value);
        });
        sink.RegisterPayloadMapper("ObligationBreachedEvent", e =>
        {
            var evt = (DomainEvents.ObligationBreachedEvent)e;
            return new ObligationBreachedEventSchema(evt.ObligationId.Value);
        });
    }
}
