namespace Whycespace.Domain.BusinessSystem.Localization.CurrencyFormat;

public readonly record struct CurrencyFormatId
{
    public Guid Value { get; }

    public CurrencyFormatId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CurrencyFormatId value must not be empty.", nameof(value));

        Value = value;
    }
}
