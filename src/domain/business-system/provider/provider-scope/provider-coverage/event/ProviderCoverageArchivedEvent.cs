using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderCoverage;

public sealed record ProviderCoverageArchivedEvent(
    [property: JsonPropertyName("AggregateId")] ProviderCoverageId ProviderCoverageId) : DomainEvent;
