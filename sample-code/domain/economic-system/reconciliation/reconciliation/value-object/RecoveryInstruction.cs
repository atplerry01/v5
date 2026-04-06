namespace Whycespace.Domain.EconomicSystem.Reconciliation.Reconciliation;

public sealed class RecoveryInstruction
{
    public Guid TransactionId { get; }
    public string Action { get; }

    public RecoveryInstruction(Guid transactionId, string action)
    {
        TransactionId = transactionId;
        Action = action;
    }
}
