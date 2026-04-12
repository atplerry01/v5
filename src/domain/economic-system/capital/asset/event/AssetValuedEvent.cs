using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Asset;

public sealed record AssetValuedEvent(
    AssetId AssetId,
    Amount PreviousValue,
    Amount NewValue,
    Currency Currency,
    Timestamp ValuedAt) : DomainEvent;
