namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

public sealed class DistributionException : DomainException
{
    public DistributionException(string message) : base("DISTRIBUTION_ERROR", message) { }
}
