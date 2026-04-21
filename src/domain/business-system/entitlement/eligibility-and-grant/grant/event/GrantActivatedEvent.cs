using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Grant;

public sealed record GrantActivatedEvent(
    [property: JsonPropertyName("AggregateId")] GrantId GrantId,
    DateTimeOffset ActivatedAt) : DomainEvent;
