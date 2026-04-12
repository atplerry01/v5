using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Limit;

public sealed record LimitDefinedEvent(
    LimitId LimitId,
    Guid AccountId,
    LimitType Type,
    Amount Threshold,
    Currency Currency,
    Timestamp DefinedAt) : DomainEvent;
