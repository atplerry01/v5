using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Pool;

public sealed record CapitalReducedEvent(
    PoolId PoolId,
    Guid SourceAccountId,
    Amount ReducedAmount,
    Amount NewPoolTotal) : DomainEvent;
