namespace Whycespace.Domain.BusinessSystem.Subscription.SubscriptionAccount;

public readonly record struct SubscriptionAccountId
{
    public Guid Value { get; }

    public SubscriptionAccountId(Guid value)
    {
        if (value == Guid.Empty)
            throw SubscriptionAccountErrors.MissingId();

        Value = value;
    }
}

public enum SubscriptionAccountStatus
{
    Created,
    Active,
    Suspended,
    Closed
}

public readonly record struct AccountHolder
{
    public Guid HolderReference { get; }
    public string HolderName { get; }

    public AccountHolder(Guid holderReference, string holderName)
    {
        if (holderReference == Guid.Empty)
            throw SubscriptionAccountErrors.MissingAccountHolder();

        if (string.IsNullOrWhiteSpace(holderName))
            throw SubscriptionAccountErrors.MissingAccountHolder();

        HolderReference = holderReference;
        HolderName = holderName;
    }
}
