namespace Whycespace.Shared.Contracts.Economic.Transaction.Transaction;

public sealed record TransactionReadModel
{
    public Guid TransactionId { get; init; }
    public string Kind { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<TransactionReferenceDto> References { get; init; } = Array.Empty<TransactionReferenceDto>();
    public DateTimeOffset? InitiatedAt { get; init; }
    public DateTimeOffset? CommittedAt { get; init; }
    public DateTimeOffset? FailedAt { get; init; }
    public string? FailureReason { get; init; }
}
