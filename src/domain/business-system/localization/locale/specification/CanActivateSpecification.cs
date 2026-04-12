namespace Whycespace.Domain.BusinessSystem.Localization.Locale;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(LocaleStatus status)
    {
        return status == LocaleStatus.Draft;
    }
}
