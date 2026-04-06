namespace Whycespace.Domain.IntelligenceSystem.Economic;

public sealed record AnalysisScope(string Value)
{
    public static readonly AnalysisScope Identity = new("identity");
    public static readonly AnalysisScope Wallet = new("wallet");
    public static readonly AnalysisScope Cluster = new("cluster");
}
