using Whycespace.Shared.Contracts.Events.Business.Agreement.PartyGovernance.Signature;
using DomainEvents = Whycespace.Domain.BusinessSystem.Agreement.PartyGovernance.Signature;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/agreement/party-governance/signature domain.
///
/// Owns the binding from Signature domain event CLR types to the
/// <see cref="EventSchemaRegistry"/>, plus the outbound payload mappers that
/// project domain events (with strongly-typed SignatureId) into the shared
/// schema records (Guid AggregateId) consumed by the projection layer.
/// </summary>
public sealed class BusinessAgreementPartyGovernanceSignatureSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "SignatureCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.SignatureCreatedEvent),
            typeof(SignatureCreatedEventSchema));

        sink.RegisterSchema(
            "SignatureSignedEvent",
            EventVersion.Default,
            typeof(DomainEvents.SignatureSignedEvent),
            typeof(SignatureSignedEventSchema));

        sink.RegisterSchema(
            "SignatureRevokedEvent",
            EventVersion.Default,
            typeof(DomainEvents.SignatureRevokedEvent),
            typeof(SignatureRevokedEventSchema));

        sink.RegisterPayloadMapper("SignatureCreatedEvent", e =>
        {
            var evt = (DomainEvents.SignatureCreatedEvent)e;
            return new SignatureCreatedEventSchema(evt.SignatureId.Value);
        });
        sink.RegisterPayloadMapper("SignatureSignedEvent", e =>
        {
            var evt = (DomainEvents.SignatureSignedEvent)e;
            return new SignatureSignedEventSchema(evt.SignatureId.Value);
        });
        sink.RegisterPayloadMapper("SignatureRevokedEvent", e =>
        {
            var evt = (DomainEvents.SignatureRevokedEvent)e;
            return new SignatureRevokedEventSchema(evt.SignatureId.Value);
        });
    }
}
