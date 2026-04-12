using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Revenue;

public sealed record RevenueRecognizedEvent(
    RevenueId RevenueId,
    Guid ContractId,
    Amount Amount,
    Currency Currency,
    Timestamp RecognizedAt) : DomainEvent;
