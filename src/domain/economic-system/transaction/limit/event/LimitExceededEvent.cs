using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Limit;

public sealed record LimitExceededEvent(
    LimitId LimitId,
    Guid TransactionId,
    Amount AttemptedAmount,
    Amount Threshold,
    Timestamp ExceededAt) : DomainEvent;
