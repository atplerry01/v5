namespace Whycespace.Domain.BusinessSystem.Localization.Translation;

public readonly record struct TranslationId
{
    public Guid Value { get; }

    public TranslationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TranslationId value must not be empty.", nameof(value));

        Value = value;
    }
}
