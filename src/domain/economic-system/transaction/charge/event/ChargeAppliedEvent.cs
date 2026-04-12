using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Charge;

public sealed record ChargeAppliedEvent(
    ChargeId ChargeId,
    Guid TransactionId,
    Amount AppliedAmount,
    Timestamp AppliedAt) : DomainEvent;
