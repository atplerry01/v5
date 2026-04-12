using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Reserve;

public sealed record ReserveReleasedEvent(
    ReserveId ReserveId,
    Guid AccountId,
    Amount ReleasedAmount,
    Timestamp ReleasedAt) : DomainEvent;
