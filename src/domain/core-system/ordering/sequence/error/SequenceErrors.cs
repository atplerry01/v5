using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Ordering.Sequence;

public static class SequenceErrors
{
    public static DomainException ValueMustBeNonNegative(long value) =>
        new DomainInvariantViolationException(
            $"Sequence value must be non-negative. Got: {value}.");

    public static DomainException RangeEndMustFollowStart(Sequence start, Sequence end) =>
        new DomainInvariantViolationException(
            $"SequenceRange end ({end.Value}) must be >= start ({start.Value}).");
}
