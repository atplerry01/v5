using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceConstraint;

public sealed record ServiceConstraintArchivedEvent(
    [property: JsonPropertyName("AggregateId")] ServiceConstraintId ServiceConstraintId) : DomainEvent;
