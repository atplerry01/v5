using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Structure.Classification;

public sealed record ClassificationDeprecatedEvent(
    [property: JsonPropertyName("AggregateId")] ClassificationId ClassificationId) : DomainEvent;
