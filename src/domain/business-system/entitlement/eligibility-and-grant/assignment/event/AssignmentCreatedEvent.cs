using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Assignment;

public sealed record AssignmentCreatedEvent(
    [property: JsonPropertyName("AggregateId")] AssignmentId AssignmentId,
    GrantRef Grant,
    AssignmentSubjectRef Subject,
    AssignmentScope Scope) : DomainEvent;
