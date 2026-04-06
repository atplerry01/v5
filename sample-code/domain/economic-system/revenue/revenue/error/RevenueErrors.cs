namespace Whycespace.Domain.EconomicSystem.Revenue.Revenue;

public sealed class RevenueException : DomainException
{
    public RevenueException(string message) : base("REVENUE_ERROR", message) { }
}
