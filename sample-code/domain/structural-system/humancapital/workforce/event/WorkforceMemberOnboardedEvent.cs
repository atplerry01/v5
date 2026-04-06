using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Workforce;

public sealed record WorkforceMemberOnboardedEvent(
    Guid WorkforceId,
    string EmploymentModel
) : DomainEvent;
