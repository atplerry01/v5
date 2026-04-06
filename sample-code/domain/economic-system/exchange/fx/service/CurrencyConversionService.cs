namespace Whycespace.Domain.EconomicSystem.Exchange.Fx;

/// <summary>
/// Domain service for currency conversion.
/// Stateless — receives FX rate aggregate and amount, returns conversion result.
/// </summary>
public sealed class CurrencyConversionService
{
    public ConversionResult Convert(FxRateAggregate rate, decimal amount, string fromCurrency, string toCurrency)
    {
        var convertedAmount = string.Equals(fromCurrency, rate.Pair.BaseCurrency, StringComparison.OrdinalIgnoreCase)
            ? rate.Convert(amount)
            : rate.ConvertInverse(amount);

        return ConversionResult.From(amount, fromCurrency, convertedAmount, toCurrency, rate.Rate, rate.Id);
    }

    public bool IsRateActive(FxRateAggregate rate) =>
        rate.Status == FxRateStatus.Active;
}
