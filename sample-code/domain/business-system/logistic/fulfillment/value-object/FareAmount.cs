namespace Whycespace.Domain.BusinessSystem.Logistic.Fulfillment;

/// <summary>
/// Represents the fare for a ride as a monetary amount.
/// </summary>
public sealed record FareAmount(decimal Value, string Currency = "USD")
{
    public static readonly FareAmount Zero = new(0m);

    public static FareAmount Of(decimal value, string currency = "USD")
    {
        if (value < 0)
            throw new ArgumentException("Fare amount cannot be negative.", nameof(value));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required.", nameof(currency));

        return new FareAmount(value, currency);
    }
}
