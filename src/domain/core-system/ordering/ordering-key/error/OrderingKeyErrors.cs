using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Ordering.OrderingKey;

public static class OrderingKeyErrors
{
    public static DomainException ValueMustNotBeEmpty() =>
        new DomainInvariantViolationException("OrderingKey value must not be null or empty.");
}
