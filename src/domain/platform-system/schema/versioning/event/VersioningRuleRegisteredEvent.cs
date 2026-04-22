using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Schema.Versioning;

public sealed record VersioningRuleRegisteredEvent(
    [property: JsonPropertyName("AggregateId")] VersioningRuleId VersioningRuleId,
    Guid SchemaRef,
    int FromVersion,
    int ToVersion,
    EvolutionClass EvolutionClass,
    IReadOnlyList<SchemaChange> ChangeSummary,
    Timestamp RegisteredAt) : DomainEvent;
