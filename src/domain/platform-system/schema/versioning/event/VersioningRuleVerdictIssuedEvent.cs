using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Schema.Versioning;

public sealed record VersioningRuleVerdictIssuedEvent(
    [property: JsonPropertyName("AggregateId")] VersioningRuleId VersioningRuleId,
    CompatibilityVerdict Verdict,
    Timestamp IssuedAt) : DomainEvent;
