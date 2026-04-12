namespace Whycespace.Domain.BusinessSystem.Localization.Translation;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(TranslationStatus status)
    {
        return status == TranslationStatus.Draft;
    }
}
