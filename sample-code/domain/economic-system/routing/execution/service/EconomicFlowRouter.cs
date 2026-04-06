namespace Whycespace.Domain.EconomicSystem.Routing.Execution;

public sealed class EconomicFlowRouter
{
    public IEnumerable<EconomicFlowInstruction> Route(
        EconomicExecutionPath executionPath,
        decimal totalAmount,
        string currency)
    {
        if (executionPath.IsEmpty())
            return Enumerable.Empty<EconomicFlowInstruction>();

        var nodes = executionPath.Path.ToList();
        var flows = new List<EconomicFlowInstruction>();

        for (var i = 0; i < nodes.Count - 1; i++)
        {
            flows.Add(new EconomicFlowInstruction(
                nodes[i],
                nodes[i + 1],
                totalAmount,
                currency));
        }

        return flows;
    }
}
