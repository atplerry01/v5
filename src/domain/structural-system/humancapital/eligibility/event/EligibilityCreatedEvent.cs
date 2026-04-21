using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Eligibility;

public sealed record EligibilityCreatedEvent(
    [property: JsonPropertyName("AggregateId")] EligibilityId EligibilityId,
    EligibilityDescriptor Descriptor) : DomainEvent;
