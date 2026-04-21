using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderCoverage;

public sealed record CoverageScopeAddedEvent(
    [property: JsonPropertyName("AggregateId")] ProviderCoverageId ProviderCoverageId,
    CoverageScope Scope) : DomainEvent;
