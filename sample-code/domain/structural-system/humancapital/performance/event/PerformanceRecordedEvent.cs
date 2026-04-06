using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Performance;

public sealed record PerformanceRecordedEvent(
    Guid RecordId,
    double Score,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd
) : DomainEvent;
