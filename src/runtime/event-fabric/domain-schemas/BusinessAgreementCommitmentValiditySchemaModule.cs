using Whycespace.Shared.Contracts.Events.Business.Agreement.Commitment.Validity;
using DomainEvents = Whycespace.Domain.BusinessSystem.Agreement.Commitment.Validity;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/agreement/commitment/validity domain.
/// </summary>
public sealed class BusinessAgreementCommitmentValiditySchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ValidityCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ValidityCreatedEvent),
            typeof(ValidityCreatedEventSchema));

        sink.RegisterSchema(
            "ValidityExpiredEvent",
            EventVersion.Default,
            typeof(DomainEvents.ValidityExpiredEvent),
            typeof(ValidityExpiredEventSchema));

        sink.RegisterSchema(
            "ValidityInvalidatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ValidityInvalidatedEvent),
            typeof(ValidityInvalidatedEventSchema));

        sink.RegisterPayloadMapper("ValidityCreatedEvent", e =>
        {
            var evt = (DomainEvents.ValidityCreatedEvent)e;
            return new ValidityCreatedEventSchema(evt.ValidityId.Value);
        });
        sink.RegisterPayloadMapper("ValidityExpiredEvent", e =>
        {
            var evt = (DomainEvents.ValidityExpiredEvent)e;
            return new ValidityExpiredEventSchema(evt.ValidityId.Value);
        });
        sink.RegisterPayloadMapper("ValidityInvalidatedEvent", e =>
        {
            var evt = (DomainEvents.ValidityInvalidatedEvent)e;
            return new ValidityInvalidatedEventSchema(evt.ValidityId.Value);
        });
    }
}
