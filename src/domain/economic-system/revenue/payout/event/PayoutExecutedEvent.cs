using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public sealed record PayoutExecutedEvent(
    string PayoutId,
    string DistributionId) : DomainEvent;
