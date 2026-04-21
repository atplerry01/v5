using Whycespace.Shared.Contracts.Events.Business.Provider.ProviderScope.ProviderCoverage;
using DomainEvents = Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderCoverage;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/provider/provider-scope/provider-coverage domain.
///
/// Owns the binding from ProviderCoverage domain event CLR types to the
/// <see cref="EventSchemaRegistry"/>, plus the outbound payload mappers that
/// project domain events (with strongly-typed ProviderCoverageId + CoverageScope
/// VO) into the shared schema records (Guid AggregateId + flattened Scope fields)
/// consumed by the projection layer. ScopeKind is serialized as the enum name
/// so the wire schema stays stable across replay and the projection layer never
/// depends on the domain CoverageScopeKind type.
/// </summary>
public sealed class BusinessProviderProviderScopeProviderCoverageSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ProviderCoverageCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProviderCoverageCreatedEvent),
            typeof(ProviderCoverageCreatedEventSchema));

        sink.RegisterSchema(
            "CoverageScopeAddedEvent",
            EventVersion.Default,
            typeof(DomainEvents.CoverageScopeAddedEvent),
            typeof(CoverageScopeAddedEventSchema));

        sink.RegisterSchema(
            "CoverageScopeRemovedEvent",
            EventVersion.Default,
            typeof(DomainEvents.CoverageScopeRemovedEvent),
            typeof(CoverageScopeRemovedEventSchema));

        sink.RegisterSchema(
            "ProviderCoverageActivatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProviderCoverageActivatedEvent),
            typeof(ProviderCoverageActivatedEventSchema));

        sink.RegisterSchema(
            "ProviderCoverageArchivedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProviderCoverageArchivedEvent),
            typeof(ProviderCoverageArchivedEventSchema));

        sink.RegisterPayloadMapper("ProviderCoverageCreatedEvent", e =>
        {
            var evt = (DomainEvents.ProviderCoverageCreatedEvent)e;
            return new ProviderCoverageCreatedEventSchema(evt.ProviderCoverageId.Value, evt.Provider.Value);
        });
        sink.RegisterPayloadMapper("CoverageScopeAddedEvent", e =>
        {
            var evt = (DomainEvents.CoverageScopeAddedEvent)e;
            return new CoverageScopeAddedEventSchema(
                evt.ProviderCoverageId.Value,
                evt.Scope.Kind.ToString(),
                evt.Scope.Descriptor);
        });
        sink.RegisterPayloadMapper("CoverageScopeRemovedEvent", e =>
        {
            var evt = (DomainEvents.CoverageScopeRemovedEvent)e;
            return new CoverageScopeRemovedEventSchema(
                evt.ProviderCoverageId.Value,
                evt.Scope.Kind.ToString(),
                evt.Scope.Descriptor);
        });
        sink.RegisterPayloadMapper("ProviderCoverageActivatedEvent", e =>
        {
            var evt = (DomainEvents.ProviderCoverageActivatedEvent)e;
            return new ProviderCoverageActivatedEventSchema(evt.ProviderCoverageId.Value);
        });
        sink.RegisterPayloadMapper("ProviderCoverageArchivedEvent", e =>
        {
            var evt = (DomainEvents.ProviderCoverageArchivedEvent)e;
            return new ProviderCoverageArchivedEventSchema(evt.ProviderCoverageId.Value);
        });
    }
}
