using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Asset;

public sealed record AssetDisposedEvent(
    AssetId AssetId,
    Amount FinalValue,
    Timestamp DisposedAt) : DomainEvent;
