using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Assignment;

public sealed record AssignmentRevokedEvent(
    [property: JsonPropertyName("AggregateId")] AssignmentId AssignmentId,
    DateTimeOffset RevokedAt) : DomainEvent;
