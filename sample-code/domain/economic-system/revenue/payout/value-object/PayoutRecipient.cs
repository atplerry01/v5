namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public sealed record PayoutRecipient
{
    public Guid RecipientId { get; }
    public string RecipientType { get; }

    public PayoutRecipient(Guid recipientId, string recipientType)
    {
        if (recipientId == Guid.Empty)
            throw new PayoutException("Recipient id is required.");

        if (string.IsNullOrWhiteSpace(recipientType))
            throw new PayoutException("Recipient type is required.");

        RecipientId = recipientId;
        RecipientType = recipientType;
    }
}
