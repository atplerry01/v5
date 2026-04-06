namespace Whycespace.Domain.SharedKernel.Primitive.Money;

public sealed record ExchangeRate
{
    public Currency From { get; }
    public Currency To { get; }
    public decimal Rate { get; }

    public ExchangeRate(Currency from, Currency to, decimal rate)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rate);

        From = from;
        To = to;
        Rate = rate;
    }

    public Money Convert(Money money)
    {
        if (money.Currency != From)
            throw new InvalidOperationException(
                $"Cannot convert {money.Currency.Code} using {From.Code}->{To.Code} rate.");

        return new Money(money.Amount * Rate, To);
    }

    public ExchangeRate Invert() => new(To, From, 1m / Rate);

    public override string ToString() => $"{From.Code}/{To.Code} = {Rate}";
}
