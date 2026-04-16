namespace Whycespace.Shared.Contracts.Economic.Revenue.Contract;

public sealed record ContractReadModel
{
    public Guid ContractId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset TermStart { get; init; }
    public DateTimeOffset TermEnd { get; init; }
    public int PartyCount { get; init; }
    public string? TerminationReason { get; init; }
    public DateTimeOffset? TerminatedAt { get; init; }
}