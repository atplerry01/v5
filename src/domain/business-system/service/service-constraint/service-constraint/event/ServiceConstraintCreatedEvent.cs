using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceConstraint;

public sealed record ServiceConstraintCreatedEvent(
    [property: JsonPropertyName("AggregateId")] ServiceConstraintId ServiceConstraintId,
    ServiceDefinitionRef ServiceDefinition,
    ConstraintKind Kind,
    ConstraintDescriptor Descriptor) : DomainEvent;
