using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Performance;

public sealed record PerformanceUpdatedEvent(
    Guid RecordId,
    double NewScore
) : DomainEvent;
