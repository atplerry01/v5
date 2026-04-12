namespace Whycespace.Domain.BusinessSystem.Localization.CurrencyFormat;

public sealed class CanDeactivateSpecification
{
    public bool IsSatisfiedBy(CurrencyFormatStatus status)
    {
        return status == CurrencyFormatStatus.Active;
    }
}
