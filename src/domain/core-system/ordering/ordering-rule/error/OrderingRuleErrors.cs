using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.CoreSystem.Ordering.OrderingKey;

namespace Whycespace.Domain.CoreSystem.Ordering.OrderingRule;

public static class OrderingRuleErrors
{
    public static DomainException TieBreakerKeyMustDiffer(OrderingKey.OrderingKey key) =>
        new DomainInvariantViolationException(
            $"TieBreaker key must differ from the parent rule key. Got duplicate key: '{key.Value}'.");

    public static DomainException TieBreakerChainExceedsMaxDepth(int maxDepth) =>
        new DomainInvariantViolationException(
            $"OrderingRule tie-breaker chain must not exceed {maxDepth} levels deep.");
}
