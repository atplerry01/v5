namespace Whycespace.Domain.BusinessSystem.Localization.Translation;

public sealed class CanReactivateSpecification
{
    public bool IsSatisfiedBy(TranslationStatus status)
    {
        return status == TranslationStatus.Suspended;
    }
}
