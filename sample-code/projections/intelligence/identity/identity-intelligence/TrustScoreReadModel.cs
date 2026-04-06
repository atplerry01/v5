namespace Whycespace.Projections.IdentityIntelligence.ReadModels;

public sealed record TrustScoreReadModel
{
    public required string IdentityId { get; init; }
    public decimal Score { get; init; }
    public required string Classification { get; init; }
    public int VerificationLevel { get; init; }
    public int PolicyViolationCount { get; init; }
    public DateTimeOffset ComputedAt { get; init; }
}
