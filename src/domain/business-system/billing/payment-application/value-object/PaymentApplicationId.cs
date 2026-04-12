namespace Whycespace.Domain.BusinessSystem.Billing.PaymentApplication;

public readonly record struct PaymentApplicationId
{
    public Guid Value { get; }

    public PaymentApplicationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PaymentApplicationId value must not be empty.", nameof(value));

        Value = value;
    }
}
