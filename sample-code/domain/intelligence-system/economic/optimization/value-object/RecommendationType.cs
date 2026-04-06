namespace Whycespace.Domain.IntelligenceSystem.Economic.Optimization;

public sealed record RecommendationType(string Value)
{
    public static readonly RecommendationType CostReduction = new("cost_reduction");
    public static readonly RecommendationType RevenueGrowth = new("revenue_growth");
    public static readonly RecommendationType RiskMitigation = new("risk_mitigation");
    public static readonly RecommendationType EfficiencyGain = new("efficiency_gain");
}
