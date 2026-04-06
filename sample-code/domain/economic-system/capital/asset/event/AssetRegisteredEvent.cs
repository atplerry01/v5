using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Asset;

public sealed record AssetRegisteredEvent(Guid AssetId) : DomainEvent;
