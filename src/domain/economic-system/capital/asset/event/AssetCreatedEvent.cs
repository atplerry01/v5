using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Asset;

public sealed record AssetCreatedEvent(
    AssetId AssetId,
    Guid OwnerId,
    Amount InitialValue,
    Currency Currency,
    Timestamp CreatedAt) : DomainEvent;
