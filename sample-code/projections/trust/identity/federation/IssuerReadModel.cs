namespace Whycespace.Projections.IdentityFederation.ReadModels;

public sealed record IssuerReadModel
{
    public required string IssuerId { get; init; }
    public required string Name { get; init; }
    public required string IssuerType { get; init; }
    public decimal TrustLevel { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset? ApprovedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record IssuerTrustReadModel
{
    public required string IssuerId { get; init; }
    public decimal BaseTrustScore { get; init; }
    public decimal AdjustedTrustScore { get; init; }
    public required string TrustStatus { get; init; }
    public required string Trend { get; init; }
    public decimal Volatility { get; init; }
    public DateTimeOffset LastEvaluatedAt { get; init; }
}

public sealed record FederationLinkReadModel
{
    public required string IdentityId { get; init; }
    public required string ExternalId { get; init; }
    public required string IssuerId { get; init; }
    public decimal Confidence { get; init; }
    public int VerificationLevel { get; init; }
    public required string Status { get; init; }
    public required string ProvenanceSource { get; init; }
    public DateTimeOffset LinkedAt { get; init; }
}
