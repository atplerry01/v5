using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

public sealed record DistributionCreatedEvent(
    string DistributionId,
    string SpvId,
    decimal TotalAmount) : DomainEvent;
