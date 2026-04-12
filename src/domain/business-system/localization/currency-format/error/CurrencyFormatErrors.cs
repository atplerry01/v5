namespace Whycespace.Domain.BusinessSystem.Localization.CurrencyFormat;

public static class CurrencyFormatErrors
{
    public static CurrencyFormatDomainException MissingId()
        => new("CurrencyFormatId is required and must not be empty.");

    public static CurrencyFormatDomainException InvalidCurrencyCode()
        => new("Currency format must define code, symbol, and decimal places.");

    public static CurrencyFormatDomainException InvalidStateTransition(CurrencyFormatStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static CurrencyFormatDomainException DuplicateCurrencyFormat(CurrencyCode code)
        => new($"Currency format for '{code.Code}' already exists.");
}

public sealed class CurrencyFormatDomainException : Exception
{
    public CurrencyFormatDomainException(string message) : base(message) { }
}
