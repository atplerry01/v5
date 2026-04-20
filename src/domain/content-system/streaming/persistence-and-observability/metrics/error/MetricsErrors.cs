using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PersistenceAndObservability.Metrics;

public static class MetricsErrors
{
    public static DomainException MetricsFinalized()
        => new("Metrics are finalized and cannot be modified.");

    public static DomainException MetricsArchived()
        => new("Metrics are archived and cannot be modified.");

    public static DomainException AlreadyFinalized()
        => new("Metrics are already finalized.");

    public static DomainException AlreadyArchived()
        => new("Metrics are already archived.");

    public static DomainException CannotArchiveUnlessFinalized()
        => new("Metrics must be finalized before archival.");

    public static DomainInvariantViolationException OrphanedMetrics()
        => new("Metrics must reference a parent stream.");
}
