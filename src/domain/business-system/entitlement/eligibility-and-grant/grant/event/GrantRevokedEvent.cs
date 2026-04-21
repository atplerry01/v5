using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Grant;

public sealed record GrantRevokedEvent(
    [property: JsonPropertyName("AggregateId")] GrantId GrantId,
    DateTimeOffset RevokedAt) : DomainEvent;
