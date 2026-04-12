namespace Whycespace.Domain.BusinessSystem.Localization.Locale;

public readonly record struct LocaleId
{
    public Guid Value { get; }

    public LocaleId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("LocaleId value must not be empty.", nameof(value));

        Value = value;
    }
}
