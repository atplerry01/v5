namespace Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Plan;

public sealed record PlanReadModel
{
    public Guid PlanId { get; init; }
    public string PlanName { get; init; } = string.Empty;
    public string PlanTier { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
