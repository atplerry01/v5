namespace Whycespace.Shared.Contracts.Events.Platform.Schema.Versioning;

public sealed record VersioningRuleRegisteredEventSchema(
    Guid AggregateId,
    Guid SchemaRef,
    int FromVersion,
    int ToVersion,
    string EvolutionClass);

public sealed record VersioningRuleVerdictIssuedEventSchema(
    Guid AggregateId,
    string Verdict);
