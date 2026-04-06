namespace Whycespace.Projections.CoreSystem.FinancialIntegrity;

public sealed record FinancialIntegrityReadModel
{
    public required string Id { get; init; }
    public required decimal TotalInflow { get; init; }
    public required decimal TotalOutflow { get; init; }
    public required decimal SystemBalance { get; init; }
    public required bool IsBalanced { get; init; }
    public required bool IsNegativeBalance { get; init; }
    public bool IsVaultConsistent { get; init; }
    public int VaultsVerified { get; init; }
    public int DiscrepanciesFound { get; init; }
    public bool IsSealed { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
    public DateTimeOffset LastEventTimestamp { get; init; }
    public long LastEventVersion { get; init; }
}
