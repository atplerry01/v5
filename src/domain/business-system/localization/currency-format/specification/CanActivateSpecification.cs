namespace Whycespace.Domain.BusinessSystem.Localization.CurrencyFormat;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(CurrencyFormatStatus status)
    {
        return status == CurrencyFormatStatus.Draft;
    }
}
