using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Eligibility;

public sealed record EligibilityGrantedEvent(
    Guid EligibilityId,
    string RuleName
) : DomainEvent;
