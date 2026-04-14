namespace Whycespace.Shared.Contracts.Economic.Transaction.Settlement;

public sealed record InitiateSettlementCommand(
    Guid SettlementId,
    decimal Amount,
    string Currency,
    string SourceReference,
    string Provider);

public sealed record CompleteSettlementCommand(
    Guid SettlementId,
    string ExternalReferenceId);

public sealed record FailSettlementCommand(
    Guid SettlementId,
    string Reason);
