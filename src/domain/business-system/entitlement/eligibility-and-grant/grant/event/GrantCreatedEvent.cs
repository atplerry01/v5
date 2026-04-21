using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Grant;

public sealed record GrantCreatedEvent(
    [property: JsonPropertyName("AggregateId")] GrantId GrantId,
    GrantSubjectRef Subject,
    GrantTargetRef Target,
    GrantScope Scope,
    GrantExpiry? Expiry) : DomainEvent;
