namespace Whycespace.Domain.BusinessSystem.Localization.Translation;

public static class TranslationErrors
{
    public static TranslationDomainException MissingId()
        => new("TranslationId is required and must not be empty.");

    public static TranslationDomainException InvalidTranslationKey()
        => new("Translation must define source language, target language, and key.");

    public static TranslationDomainException InvalidStateTransition(TranslationStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static TranslationDomainException DuplicateTranslation(TranslationKey key)
        => new($"Translation for '{key.SourceLanguage}' to '{key.TargetLanguage}' with key '{key.Key}' already exists.");
}

public sealed class TranslationDomainException : Exception
{
    public TranslationDomainException(string message) : base(message) { }
}
