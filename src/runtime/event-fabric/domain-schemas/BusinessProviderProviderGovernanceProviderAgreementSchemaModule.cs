using Whycespace.Shared.Contracts.Events.Business.Provider.ProviderGovernance.ProviderAgreement;
using DomainEvents = Whycespace.Domain.BusinessSystem.Provider.ProviderGovernance.ProviderAgreement;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/provider/provider-governance/provider-agreement domain.
/// </summary>
public sealed class BusinessProviderProviderGovernanceProviderAgreementSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ProviderAgreementCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProviderAgreementCreatedEvent),
            typeof(ProviderAgreementCreatedEventSchema));

        sink.RegisterSchema(
            "ProviderAgreementActivatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProviderAgreementActivatedEvent),
            typeof(ProviderAgreementActivatedEventSchema));

        sink.RegisterSchema(
            "ProviderAgreementSuspendedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProviderAgreementSuspendedEvent),
            typeof(ProviderAgreementSuspendedEventSchema));

        sink.RegisterSchema(
            "ProviderAgreementTerminatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProviderAgreementTerminatedEvent),
            typeof(ProviderAgreementTerminatedEventSchema));

        sink.RegisterPayloadMapper("ProviderAgreementCreatedEvent", e =>
        {
            var evt = (DomainEvents.ProviderAgreementCreatedEvent)e;
            return new ProviderAgreementCreatedEventSchema(
                evt.ProviderAgreementId.Value,
                evt.Provider.Value,
                evt.Contract.HasValue ? evt.Contract.Value.Value : (Guid?)null,
                evt.Effective.HasValue ? evt.Effective.Value.StartsAt : (DateTimeOffset?)null,
                evt.Effective.HasValue ? evt.Effective.Value.EndsAt : null);
        });
        sink.RegisterPayloadMapper("ProviderAgreementActivatedEvent", e =>
        {
            var evt = (DomainEvents.ProviderAgreementActivatedEvent)e;
            return new ProviderAgreementActivatedEventSchema(
                evt.ProviderAgreementId.Value,
                evt.Effective.StartsAt,
                evt.Effective.EndsAt);
        });
        sink.RegisterPayloadMapper("ProviderAgreementSuspendedEvent", e =>
        {
            var evt = (DomainEvents.ProviderAgreementSuspendedEvent)e;
            return new ProviderAgreementSuspendedEventSchema(evt.ProviderAgreementId.Value, evt.SuspendedAt);
        });
        sink.RegisterPayloadMapper("ProviderAgreementTerminatedEvent", e =>
        {
            var evt = (DomainEvents.ProviderAgreementTerminatedEvent)e;
            return new ProviderAgreementTerminatedEventSchema(evt.ProviderAgreementId.Value, evt.TerminatedAt);
        });
    }
}
