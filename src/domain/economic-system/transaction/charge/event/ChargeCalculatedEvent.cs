using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Charge;

public sealed record ChargeCalculatedEvent(
    ChargeId ChargeId,
    Guid TransactionId,
    ChargeType Type,
    Amount BaseAmount,
    Amount ChargeAmount,
    Currency Currency,
    Timestamp CalculatedAt) : DomainEvent;
