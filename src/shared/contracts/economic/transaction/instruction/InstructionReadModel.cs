namespace Whycespace.Shared.Contracts.Economic.Transaction.Instruction;

public sealed record InstructionReadModel
{
    public Guid InstructionId { get; init; }
    public Guid FromAccountId { get; init; }
    public Guid ToAccountId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ExecutedAt { get; init; }
    public DateTimeOffset? CancelledAt { get; init; }
    public string CancellationReason { get; init; } = string.Empty;
}
