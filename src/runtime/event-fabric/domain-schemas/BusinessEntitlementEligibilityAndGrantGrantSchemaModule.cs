using Whycespace.Shared.Contracts.Events.Business.Entitlement.EligibilityAndGrant.Grant;
using DomainEvents = Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Grant;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/entitlement/eligibility-and-grant/grant domain.
///
/// Owns the binding from Grant domain event CLR types to the
/// <see cref="EventSchemaRegistry"/>, plus the outbound payload mappers that
/// project domain events (with strongly-typed GrantId) into the shared
/// schema records (Guid AggregateId) consumed by the projection layer.
/// </summary>
public sealed class BusinessEntitlementEligibilityAndGrantGrantSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "GrantCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.GrantCreatedEvent),
            typeof(GrantCreatedEventSchema));

        sink.RegisterSchema(
            "GrantActivatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.GrantActivatedEvent),
            typeof(GrantActivatedEventSchema));

        sink.RegisterSchema(
            "GrantRevokedEvent",
            EventVersion.Default,
            typeof(DomainEvents.GrantRevokedEvent),
            typeof(GrantRevokedEventSchema));

        sink.RegisterSchema(
            "GrantExpiredEvent",
            EventVersion.Default,
            typeof(DomainEvents.GrantExpiredEvent),
            typeof(GrantExpiredEventSchema));

        sink.RegisterPayloadMapper("GrantCreatedEvent", e =>
        {
            var evt = (DomainEvents.GrantCreatedEvent)e;
            return new GrantCreatedEventSchema(
                evt.GrantId.Value,
                evt.Subject.Value,
                evt.Target.Value,
                evt.Scope.Value,
                evt.Expiry?.ExpiresAt);
        });
        sink.RegisterPayloadMapper("GrantActivatedEvent", e =>
        {
            var evt = (DomainEvents.GrantActivatedEvent)e;
            return new GrantActivatedEventSchema(evt.GrantId.Value, evt.ActivatedAt);
        });
        sink.RegisterPayloadMapper("GrantRevokedEvent", e =>
        {
            var evt = (DomainEvents.GrantRevokedEvent)e;
            return new GrantRevokedEventSchema(evt.GrantId.Value, evt.RevokedAt);
        });
        sink.RegisterPayloadMapper("GrantExpiredEvent", e =>
        {
            var evt = (DomainEvents.GrantExpiredEvent)e;
            return new GrantExpiredEventSchema(evt.GrantId.Value, evt.ExpiredAt);
        });
    }
}
