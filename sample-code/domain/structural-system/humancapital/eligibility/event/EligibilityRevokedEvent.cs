using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Eligibility;

public sealed record EligibilityRevokedEvent(
    Guid EligibilityId,
    string Reason
) : DomainEvent;
