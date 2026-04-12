using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Reserve;

public sealed record ReserveCreatedEvent(
    ReserveId ReserveId,
    Guid AccountId,
    Amount ReservedAmount,
    Currency Currency,
    Timestamp ReservedAt,
    Timestamp ExpiresAt) : DomainEvent;
