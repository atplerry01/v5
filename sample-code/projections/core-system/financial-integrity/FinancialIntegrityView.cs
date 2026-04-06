namespace Whycespace.Projections.CoreSystem.FinancialIntegrity;

public sealed record FinancialIntegrityView
{
    public required string Id { get; init; }
    public required decimal TotalInflow { get; init; }
    public required decimal TotalOutflow { get; init; }
    public required decimal SystemBalance { get; init; }
    public required bool IsBalanced { get; init; }
    public required bool IsHealthy { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
