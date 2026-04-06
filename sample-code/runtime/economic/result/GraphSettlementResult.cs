namespace Whycespace.Runtime.Economic.Result;

public sealed record GraphSettlementResult
{
    public required bool IsSuccess { get; init; }
    public Guid? TransactionId { get; init; }
    public string? ErrorMessage { get; init; }

    public static GraphSettlementResult Success(Guid transactionId) =>
        new() { IsSuccess = true, TransactionId = transactionId };

    public static GraphSettlementResult Failed(string error) =>
        new() { IsSuccess = false, ErrorMessage = error };
}
