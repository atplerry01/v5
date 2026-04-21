using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Structure.Classification;

public sealed record ClassificationDefinedEvent(
    [property: JsonPropertyName("AggregateId")] ClassificationId ClassificationId,
    ClassificationDescriptor Descriptor) : DomainEvent;
