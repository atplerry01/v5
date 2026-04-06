namespace Whycespace.Domain.EconomicSystem.Routing.Execution;

public sealed class EconomicFlowInstruction
{
    public Guid FromEntityId { get; }
    public Guid ToEntityId { get; }
    public decimal Amount { get; }
    public string Currency { get; }

    public EconomicFlowInstruction(Guid from, Guid to, decimal amount, string currency)
    {
        FromEntityId = from;
        ToEntityId = to;
        Amount = amount;
        Currency = currency;
    }
}
