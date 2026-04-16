using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Revenue;

public static class RevenueErrors
{
    public static DomainException AlreadyRecorded(RevenueId id) =>
        new($"Revenue {id.Value} is already recorded. Re-recording is not permitted.");
}
