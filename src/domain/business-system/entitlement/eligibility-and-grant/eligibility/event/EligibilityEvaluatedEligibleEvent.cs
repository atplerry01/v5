using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Eligibility;

public sealed record EligibilityEvaluatedEligibleEvent(
    [property: JsonPropertyName("AggregateId")] EligibilityId EligibilityId,
    DateTimeOffset EvaluatedAt) : DomainEvent;
