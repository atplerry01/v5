using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.DecisionSystem.Evaluation.Performance;

public sealed record PerformanceCreatedEvent(
    [property: JsonPropertyName("AggregateId")] PerformanceId PerformanceId,
    PerformanceDescriptor Descriptor) : DomainEvent;
