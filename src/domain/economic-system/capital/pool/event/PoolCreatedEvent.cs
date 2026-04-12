using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Pool;

public sealed record PoolCreatedEvent(
    PoolId PoolId,
    Currency Currency,
    Timestamp CreatedAt) : DomainEvent;
