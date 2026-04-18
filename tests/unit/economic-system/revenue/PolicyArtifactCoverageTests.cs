namespace Whycespace.Tests.Unit.EconomicSystem.Revenue;

/// <summary>
/// Phase 3.5 T3.5.3 / T3.5.5 — proves that every policy id registered by
/// RevenuePolicyModule (including the Phase 3 additions) has a matching
/// rule line inside the corresponding `.rego` file under
/// `infrastructure/policy/domain/economic/revenue/`. Catches the
/// "registered binding without a backing artifact" drift class.
/// </summary>
public sealed class PolicyArtifactCoverageTests
{
    private static readonly string RepoRoot =
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".."));

    private static readonly string RevenuePolicyDir =
        Path.Combine(RepoRoot, "infrastructure", "policy", "domain", "economic", "revenue");

    [Theory]
    [InlineData("whyce.economic.revenue.distribution.create",      "distribution.rego")]
    [InlineData("whyce.economic.revenue.distribution.confirm",     "distribution.rego")]
    [InlineData("whyce.economic.revenue.distribution.mark_paid",   "distribution.rego")]
    [InlineData("whyce.economic.revenue.distribution.mark_failed", "distribution.rego")]
    [InlineData("whyce.economic.revenue.payout.execute",           "payout.rego")]
    [InlineData("whyce.economic.revenue.payout.mark_executed",     "payout.rego")]
    [InlineData("whyce.economic.revenue.payout.mark_failed",       "payout.rego")]
    public void EveryRegisteredPolicyId_HasMatchingRegoArtifact(string policyId, string regoFile)
    {
        var path = Path.Combine(RevenuePolicyDir, regoFile);
        Assert.True(File.Exists(path), $"Missing rego artifact: {path}");

        var content = File.ReadAllText(path);
        Assert.Contains($"\"{policyId}\"", content);
    }

    [Fact]
    public void IdempotencyIndexMigration_ExistsAndIsIdempotent()
    {
        var path = Path.Combine(
            RepoRoot,
            "infrastructure", "data", "postgres", "projections",
            "economic", "revenue", "payout",
            "002_business_idempotency_index.sql");

        Assert.True(File.Exists(path), $"Missing migration: {path}");
        var content = File.ReadAllText(path);
        Assert.Contains("CREATE INDEX IF NOT EXISTS", content);
        Assert.Contains("idempotencyKey", content);
    }
}
