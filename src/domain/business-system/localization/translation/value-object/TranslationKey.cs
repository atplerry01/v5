namespace Whycespace.Domain.BusinessSystem.Localization.Translation;

public readonly record struct TranslationKey
{
    public string SourceLanguage { get; }
    public string TargetLanguage { get; }
    public string Key { get; }

    public TranslationKey(string sourceLanguage, string targetLanguage, string key)
    {
        if (string.IsNullOrWhiteSpace(sourceLanguage))
            throw new ArgumentException("Source language must not be empty.", nameof(sourceLanguage));

        if (string.IsNullOrWhiteSpace(targetLanguage))
            throw new ArgumentException("Target language must not be empty.", nameof(targetLanguage));

        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Translation key must not be empty.", nameof(key));

        SourceLanguage = sourceLanguage;
        TargetLanguage = targetLanguage;
        Key = key;
    }
}
