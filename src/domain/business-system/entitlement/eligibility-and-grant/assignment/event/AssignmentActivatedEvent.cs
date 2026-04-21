using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Assignment;

public sealed record AssignmentActivatedEvent(
    [property: JsonPropertyName("AggregateId")] AssignmentId AssignmentId,
    DateTimeOffset ActivatedAt) : DomainEvent;
