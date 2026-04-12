namespace Whycespace.Domain.BusinessSystem.Localization.Translation;

public sealed class CanSuspendSpecification
{
    public bool IsSatisfiedBy(TranslationStatus status)
    {
        return status == TranslationStatus.Active;
    }
}
