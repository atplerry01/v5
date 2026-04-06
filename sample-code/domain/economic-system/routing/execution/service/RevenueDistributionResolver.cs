namespace Whycespace.Domain.EconomicSystem.Routing.Execution;

public sealed class RevenueDistributionResolver
{
    public IReadOnlyDictionary<Guid, decimal> Distribute(
        EconomicExecutionPath path,
        decimal totalAmount)
    {
        var result = new Dictionary<Guid, decimal>();

        if (path.IsEmpty())
            return result;

        var nodes = path.Path.ToList();
        var share = totalAmount / nodes.Count;

        foreach (var node in nodes)
        {
            result[node] = share;
        }

        return result;
    }
}
