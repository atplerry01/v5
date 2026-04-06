using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Workforce;

public sealed record WorkforceAvailabilityChangedEvent(
    Guid WorkforceId,
    string NewAvailability
) : DomainEvent;
