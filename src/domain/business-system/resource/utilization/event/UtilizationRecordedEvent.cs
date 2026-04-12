namespace Whycespace.Domain.BusinessSystem.Resource.Utilization;

public sealed record UtilizationRecordedEvent(
    UtilizationId UtilizationId,
    UsageAmount UsageAmount);
