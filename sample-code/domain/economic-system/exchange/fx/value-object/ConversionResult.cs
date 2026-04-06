namespace Whycespace.Domain.EconomicSystem.Exchange.Fx;

/// <summary>
/// Immutable result of a currency conversion.
/// </summary>
public sealed record ConversionResult
{
    public decimal OriginalAmount { get; }
    public string OriginalCurrency { get; }
    public decimal ConvertedAmount { get; }
    public string TargetCurrency { get; }
    public decimal RateApplied { get; }
    public Guid FxRateId { get; }

    private ConversionResult(decimal originalAmount, string originalCurrency, decimal convertedAmount, string targetCurrency, decimal rateApplied, Guid fxRateId)
    {
        OriginalAmount = originalAmount;
        OriginalCurrency = originalCurrency;
        ConvertedAmount = convertedAmount;
        TargetCurrency = targetCurrency;
        RateApplied = rateApplied;
        FxRateId = fxRateId;
    }

    public static ConversionResult From(decimal originalAmount, string originalCurrency, decimal convertedAmount, string targetCurrency, decimal rateApplied, Guid fxRateId) =>
        new(originalAmount, originalCurrency, convertedAmount, targetCurrency, rateApplied, fxRateId);
}
