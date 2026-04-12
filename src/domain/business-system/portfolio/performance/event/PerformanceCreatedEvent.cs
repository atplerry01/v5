namespace Whycespace.Domain.BusinessSystem.Portfolio.Performance;

public sealed record PerformanceCreatedEvent(
    PerformanceId PerformanceId,
    PerformanceName PerformanceName);
