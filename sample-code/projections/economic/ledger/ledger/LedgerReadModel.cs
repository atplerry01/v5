namespace Whycespace.Projections.Economic;

/// <summary>
/// Maps ledger entries ↔ policy decision hashes for audit queries.
/// Projected from whyce.observability.policy.decision.anchored events
/// that carry economic context.
/// Key = "ledger-policy-link:{decisionHash}".
/// </summary>
public sealed record LedgerPolicyLinkReadModel
{
    public required string DecisionHash { get; init; }
    public required string AccountId { get; init; }
    public required string AssetId { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required string TransactionType { get; init; }
    public required string PolicyId { get; init; }
    public required string Decision { get; init; }
    public required string SubjectId { get; init; }
    public string? BlockId { get; init; }
    public string? BlockHash { get; init; }
    public required DateTimeOffset AnchoredAt { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
    public long LastEventVersion { get; init; }

    public static string KeyFor(string decisionHash) => $"ledger-policy-link:{decisionHash}";
    public static string KeyByAccount(string accountId) => $"ledger-policy-links-by-account:{accountId}";
}

/// <summary>
/// Index of ledger-policy links by account for audit queries.
/// </summary>
public sealed record LedgerPolicyLinkIndexReadModel
{
    public required string AccountId { get; init; }
    public List<string> DecisionHashes { get; init; } = [];
    public int LinkCount { get; init; }
    public DateTimeOffset LastUpdated { get; init; }

    public static string KeyFor(string accountId) => $"ledger-policy-links-by-account:{accountId}";
}
