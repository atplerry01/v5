using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Pool;

public sealed record CapitalAggregatedEvent(
    PoolId PoolId,
    Guid SourceAccountId,
    Amount AggregatedAmount,
    Amount NewPoolTotal) : DomainEvent;
