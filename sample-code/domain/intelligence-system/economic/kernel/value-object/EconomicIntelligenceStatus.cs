namespace Whycespace.Domain.IntelligenceSystem.Economic;

public sealed record EconomicIntelligenceStatus(string Value)
{
    public static readonly EconomicIntelligenceStatus Pending = new("pending");
    public static readonly EconomicIntelligenceStatus Completed = new("completed");
    public static readonly EconomicIntelligenceStatus Failed = new("failed");

    public bool IsTerminal => this == Completed || this == Failed;
}
