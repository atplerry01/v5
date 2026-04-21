namespace Whycespace.Shared.Contracts.Events.Business.Provider.ProviderScope.ProviderCoverage;

public sealed record ProviderCoverageCreatedEventSchema(
    Guid AggregateId,
    Guid ProviderId);

// CoverageScope VO (Kind + Descriptor) is flattened onto the event schema so the
// projection layer doesn't depend on the domain value-object type. ScopeKind is
// transported as the enum name (stringified) to keep the schema stable across
// projection re-plays even if the CoverageScopeKind enum is extended.
public sealed record CoverageScopeAddedEventSchema(
    Guid AggregateId,
    string ScopeKind,
    string ScopeDescriptor);

public sealed record CoverageScopeRemovedEventSchema(
    Guid AggregateId,
    string ScopeKind,
    string ScopeDescriptor);

public sealed record ProviderCoverageActivatedEventSchema(Guid AggregateId);

public sealed record ProviderCoverageArchivedEventSchema(Guid AggregateId);
