namespace Whycespace.Domain.BusinessSystem.Portfolio.Allocation;

public sealed record AllocationCreatedEvent(
    AllocationId AllocationId,
    AllocationPortfolioReference PortfolioReference,
    TargetReference TargetReference,
    AllocationWeight Weight);
