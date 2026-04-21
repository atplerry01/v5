using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Operator;

public sealed record OperatorCreatedEvent(
    [property: JsonPropertyName("AggregateId")] OperatorId OperatorId,
    OperatorDescriptor Descriptor) : DomainEvent;
