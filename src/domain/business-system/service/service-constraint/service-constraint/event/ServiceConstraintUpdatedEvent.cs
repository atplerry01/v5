using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceConstraint;

public sealed record ServiceConstraintUpdatedEvent(
    [property: JsonPropertyName("AggregateId")] ServiceConstraintId ServiceConstraintId,
    ConstraintKind Kind,
    ConstraintDescriptor Descriptor) : DomainEvent;
