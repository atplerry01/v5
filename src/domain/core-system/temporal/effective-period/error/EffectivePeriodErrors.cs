using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Temporal.EffectivePeriod;

public static class EffectivePeriodErrors
{
    public static DomainException ToMustFollowFrom(DateTimeOffset from, DateTimeOffset to) =>
        new DomainInvariantViolationException(
            $"EffectivePeriod 'to' ({to:O}) must be after 'from' ({from:O}).");
}
