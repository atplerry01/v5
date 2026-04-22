using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.DecisionSystem.Evaluation.Reputation;

public sealed record ReputationCreatedEvent(
    [property: JsonPropertyName("AggregateId")] ReputationId ReputationId,
    ReputationDescriptor Descriptor) : DomainEvent;
