using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Workforce;

public sealed record WorkforceCapacityUpdatedEvent(
    Guid WorkforceId,
    int NewCapacity
) : DomainEvent;
