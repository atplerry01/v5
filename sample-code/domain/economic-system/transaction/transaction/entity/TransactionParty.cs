namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

public sealed class TransactionParty : Entity
{
    public Guid TransactionId { get; private set; }
    public Guid WalletId { get; private set; }
    public string Role { get; private set; } = string.Empty;

    public static TransactionParty Source(Guid id, Guid transactionId, Guid walletId)
    {
        return new TransactionParty
        {
            Id = id,
            TransactionId = transactionId,
            WalletId = walletId,
            Role = "Source"
        };
    }

    public static TransactionParty Destination(Guid id, Guid transactionId, Guid walletId)
    {
        return new TransactionParty
        {
            Id = id,
            TransactionId = transactionId,
            WalletId = walletId,
            Role = "Destination"
        };
    }
}
