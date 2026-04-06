namespace Whycespace.Domain.EconomicSystem.Capital.Vault;

public sealed class VaultTransfer
{
    public Guid Id { get; }
    public Guid RecipientId { get; }
    public decimal Amount { get; }
    public string CurrencyCode { get; }

    public VaultTransfer(Guid transferId, Guid recipientId, decimal amount, string currencyCode)
    {
        Id = transferId;
        RecipientId = recipientId;
        Amount = amount;
        CurrencyCode = currencyCode;
    }
}
