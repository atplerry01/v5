namespace Whycespace.Domain.BusinessSystem.Localization.Locale;

public readonly record struct LocaleCode
{
    public string Language { get; }
    public string Region { get; }

    public LocaleCode(string language, string region)
    {
        if (string.IsNullOrWhiteSpace(language))
            throw new ArgumentException("Language must not be empty.", nameof(language));

        if (string.IsNullOrWhiteSpace(region))
            throw new ArgumentException("Region must not be empty.", nameof(region));

        Language = language;
        Region = region;
    }
}
