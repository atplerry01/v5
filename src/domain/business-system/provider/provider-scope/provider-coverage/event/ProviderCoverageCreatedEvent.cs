using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderCoverage;

public sealed record ProviderCoverageCreatedEvent(
    [property: JsonPropertyName("AggregateId")] ProviderCoverageId ProviderCoverageId,
    ClusterProviderRef Provider) : DomainEvent;
