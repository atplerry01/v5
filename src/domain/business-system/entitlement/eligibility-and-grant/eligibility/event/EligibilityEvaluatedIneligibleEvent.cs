using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Eligibility;

public sealed record EligibilityEvaluatedIneligibleEvent(
    [property: JsonPropertyName("AggregateId")] EligibilityId EligibilityId,
    IneligibilityReason Reason,
    DateTimeOffset EvaluatedAt) : DomainEvent;
