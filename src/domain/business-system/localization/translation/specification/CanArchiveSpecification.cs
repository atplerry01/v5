namespace Whycespace.Domain.BusinessSystem.Localization.Translation;

public sealed class CanArchiveSpecification
{
    public bool IsSatisfiedBy(TranslationStatus status)
    {
        return status is TranslationStatus.Active or TranslationStatus.Suspended;
    }
}
