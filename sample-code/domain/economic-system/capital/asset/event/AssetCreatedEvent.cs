using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Asset;

public sealed record AssetCreatedEvent(Guid AssetId) : DomainEvent;
