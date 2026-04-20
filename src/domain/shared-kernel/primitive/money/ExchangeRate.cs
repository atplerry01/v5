using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.SharedKernel.Primitive.Money;

public readonly record struct ExchangeRate
{
    public decimal Rate { get; }
    public Currency From { get; }
    public Currency To { get; }

    public ExchangeRate(decimal rate, Currency from, Currency to)
    {
        Guard.Against(rate <= 0m, "ExchangeRate.Rate must be greater than zero.");
        Guard.Against(from == to, "ExchangeRate.From and ExchangeRate.To must differ.");
        Rate = rate;
        From = from;
        To = to;
    }
}
