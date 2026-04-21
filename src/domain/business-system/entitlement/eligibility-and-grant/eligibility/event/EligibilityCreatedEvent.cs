using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Eligibility;

public sealed record EligibilityCreatedEvent(
    [property: JsonPropertyName("AggregateId")] EligibilityId EligibilityId,
    EligibilitySubjectRef Subject,
    EligibilityTargetRef Target,
    EligibilityScope Scope) : DomainEvent;
