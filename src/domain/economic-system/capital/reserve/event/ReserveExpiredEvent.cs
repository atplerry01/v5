using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Reserve;

public sealed record ReserveExpiredEvent(
    ReserveId ReserveId,
    Guid AccountId,
    Amount ExpiredAmount,
    Timestamp ExpiredAt) : DomainEvent;
