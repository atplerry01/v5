using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Exchange.Rate;

/// <summary>
/// D-VO-TYPING-01 — typed FX rate VO replacing the raw <c>decimal RateValue</c>
/// previously embedded on <see cref="ExchangeRateAggregate"/>. Enforces a
/// strictly-positive rate at construction; rates of zero or negative value
/// are economically invalid regardless of direction.
/// </summary>
public readonly record struct ExchangeRate
{
    public decimal Value { get; }

    public ExchangeRate(decimal value)
    {
        Guard.Against(value <= 0m, "ExchangeRate must be strictly positive.");
        Value = value;
    }
}
