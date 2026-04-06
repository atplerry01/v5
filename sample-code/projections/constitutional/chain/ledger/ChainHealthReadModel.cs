namespace Whycespace.Projections.Chain;

/// <summary>
/// Chain health derived from block events.
/// Tracks continuity validity, hash consistency, and missing sequence detection.
/// Key = "chain-health" (singleton projection).
/// </summary>
public sealed record ChainHealthReadModel
{
    public required bool ContinuityValid { get; init; }
    public required bool HashConsistent { get; init; }
    public required long ExpectedNextSequence { get; init; }
    public required string ExpectedPreviousHash { get; init; }
    public int MissingSequenceCount { get; init; }
    public List<long> MissingSequences { get; init; } = [];
    public int ContinuityBreakCount { get; init; }
    public required DateTimeOffset LastChecked { get; init; }
    public long LastEventVersion { get; init; }

    public bool IsHealthy => ContinuityValid && HashConsistent && MissingSequenceCount == 0;

    public const string Key = "chain-health";
}
