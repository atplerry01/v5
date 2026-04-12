namespace Whycespace.Domain.BusinessSystem.Localization.Locale;

public sealed class CanDeactivateSpecification
{
    public bool IsSatisfiedBy(LocaleStatus status)
    {
        return status == LocaleStatus.Active;
    }
}
