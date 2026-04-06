namespace Whycespace.Engines.T3I.IntelligenceIntegrity;

public sealed record IntegrityResultDto
{
    public required string IntegrityId { get; init; }
    public required string IdentityId { get; init; }
    public string? WalletId { get; init; }
    public required string Scope { get; init; }
    public required decimal IntegrityScore { get; init; }
    public required decimal CalibratedConfidence { get; init; }
    public required bool ConflictDetected { get; init; }
    public string? ConflictReason { get; init; }
    public required DateTimeOffset WindowStart { get; init; }
    public required DateTimeOffset WindowEnd { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public required string CorrelationId { get; init; }
}
