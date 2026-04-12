using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Limit;

public sealed record LimitCheckedEvent(
    LimitId LimitId,
    Guid TransactionId,
    Amount TransactionAmount,
    Amount CurrentUtilization,
    Timestamp CheckedAt) : DomainEvent;
