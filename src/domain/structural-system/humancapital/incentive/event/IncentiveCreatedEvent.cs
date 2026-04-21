using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Incentive;

public sealed record IncentiveCreatedEvent(
    [property: JsonPropertyName("AggregateId")] IncentiveId IncentiveId,
    IncentiveDescriptor Descriptor) : DomainEvent;
