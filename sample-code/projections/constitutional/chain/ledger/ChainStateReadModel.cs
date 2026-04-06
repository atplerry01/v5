namespace Whycespace.Projections.Chain;

/// <summary>
/// Chain state derived from whyce.chain.block.created events ONLY.
/// Tracks current head, block height, and last timestamp.
/// Key = "chain-state" (singleton projection).
/// </summary>
public sealed record ChainStateReadModel
{
    public required string CurrentHeadHash { get; init; }
    public required string CurrentHeadBlockId { get; init; }
    public required long BlockHeight { get; init; }
    public required DateTimeOffset LastBlockTimestamp { get; init; }
    public required DateTimeOffset LastUpdated { get; init; }
    public long LastEventVersion { get; init; }

    public const string Key = "chain-state";
}
