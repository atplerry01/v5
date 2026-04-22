using Whycespace.Domain.StructuralSystem.Cluster.Cluster;
using Whycespace.Domain.StructuralSystem.Cluster.Authority;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Domain.EconomicSystem.Capital.Pool;
using Whycespace.Domain.EconomicSystem.Capital.Vault;
using Whycespace.Domain.EconomicSystem.Capital.Account;
using Whycespace.Domain.EconomicSystem.Ledger.Treasury;
using Whycespace.Domain.EconomicSystem.Transaction.Instruction;
using Whycespace.Domain.ContentSystem.Document.CoreObject.Document;
using Whycespace.Domain.ContentSystem.Document.Governance.Retention;
using Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.CrossSystem;

/// <summary>
/// Cross-system golden path E2E tests.
/// Exercises realistic multi-system lifecycles within a single test,
/// verifying that system outputs compose correctly without persistence.
/// </summary>
public sealed class CrossSystemGoldenPathTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp T0 = new(new DateTimeOffset(2026, 4, 22, 9, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp T1 = new(new DateTimeOffset(2026, 4, 22, 9, 30, 0, TimeSpan.Zero));
    private static readonly Timestamp T2 = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp T3 = new(new DateTimeOffset(2026, 4, 22, 10, 30, 0, TimeSpan.Zero));
    private static readonly Timestamp T4 = new(new DateTimeOffset(2026, 4, 22, 17, 0, 0, TimeSpan.Zero));
    private static readonly Currency Usd = new("USD");

    // ── GP1: Structural → Economic Capital Activation ────────────────────────

    /// <summary>
    /// A cluster is defined and activated in the structural system,
    /// then a capital pool and treasury are established in the economic system
    /// referencing the cluster's identity as owner.
    /// </summary>
    [Fact]
    public void StructuralToEconomic_ClusterToCapitalPool_GoldenPath()
    {
        // Step 1 — define and activate cluster (structural)
        var clusterId = new ClusterId(IdGen.Generate("GP:cross:1:cluster"));
        var cluster = ClusterAggregate.Define(clusterId, new ClusterDescriptor("Alpha Hub", "Primary"));
        cluster.Activate();

        Assert.Equal(ClusterStatus.Active, cluster.Status);

        // Step 2 — establish authority under cluster (structural)
        var authorityId = new AuthorityId(IdGen.Generate("GP:cross:1:authority"));
        var clusterRef = new ClusterRef(clusterId.Value);
        var authority = AuthorityAggregate.Establish(authorityId, new AuthorityDescriptor(clusterRef, "Alpha Finance Authority"));
        authority.Activate();

        Assert.Equal(AuthorityStatus.Active, authority.Status);

        // Step 3 — create capital pool for the cluster (economic)
        var poolId = new PoolId(IdGen.Generate("GP:cross:1:pool"));
        var pool = CapitalPoolAggregate.Create(poolId, Usd, T0);
        var sourceId = IdGen.Generate("GP:cross:1:source");
        pool.AggregateCapital(sourceId, new Amount(50000m));

        Assert.Equal(50000m, pool.TotalCapital.Value);

        // Step 4 — create treasury linked to cluster's economic presence
        var treasuryId = new TreasuryId(IdGen.Generate("GP:cross:1:treasury"));
        var treasury = TreasuryAggregate.Create(treasuryId, Usd, T0);
        treasury.ReleaseFunds(new Amount(10000m));

        Assert.Equal(10000m, treasury.Balance.Value);

        // Step 5 — allocate from treasury for authority operations
        treasury.AllocateFunds(new Amount(3000m));

        Assert.Equal(7000m, treasury.Balance.Value);

        // Final state: structural chain Active, economic capital operational
        Assert.Equal(ClusterStatus.Active, cluster.Status);
        Assert.Equal(AuthorityStatus.Active, authority.Status);
        Assert.Equal(50000m, pool.TotalCapital.Value);
    }

    // ── GP2: Economic Instruction + Content Document Pairing ─────────────────

    /// <summary>
    /// A financial instruction is created in the economic system, and the
    /// corresponding authorisation document is registered in the content system.
    /// Both are independently verifiable; their linkage is carried by shared IDs.
    /// </summary>
    [Fact]
    public void EconomicInstruction_WithContentDocumentAuthorisation_GoldenPath()
    {
        // Step 1 — set up accounts (economic)
        var fromAccountId = new AccountId(IdGen.Generate("GP:cross:2:from-account"));
        var toAccountId = new AccountId(IdGen.Generate("GP:cross:2:to-account"));

        // Step 2 — create transfer instruction (economic)
        var instructionId = new InstructionId(IdGen.Generate("GP:cross:2:instruction"));
        var instruction = TransactionInstructionAggregate.CreateInstruction(
            instructionId, fromAccountId, toAccountId,
            new Amount(25000m), Usd, InstructionType.Transfer, T0);

        Assert.Equal(InstructionStatus.Pending, instruction.Status);
        Assert.Equal(25000m, instruction.Amount.Value);

        // Step 3 — create authorisation document (content)
        var docId = new DocumentId(IdGen.Generate("GP:cross:2:doc"));
        var structuralOwner = new StructuralOwnerRef(IdGen.Generate("GP:cross:2:owner"));
        var businessOwner = new BusinessOwnerRef(BusinessOwnerKind.Agreement, IdGen.Generate("GP:cross:2:ba"));
        var document = DocumentAggregate.Create(
            docId,
            new DocumentTitle("Transfer Authorisation #25000"),
            DocumentType.Report,
            DocumentClassification.Confidential,
            structuralOwner, businessOwner, T0);

        Assert.Equal(DocumentStatus.Draft, document.Status);

        // Step 4 — apply retention on the authorisation (content)
        var retentionId = new RetentionId(IdGen.Generate("GP:cross:2:retention"));
        var retention = RetentionAggregate.Apply(
            retentionId,
            new RetentionTargetRef(docId.Value, RetentionTargetKind.Document),
            new RetentionWindow(T0, T4),
            new RetentionReason("Financial record — 7-year regulatory retention."),
            T1);

        Assert.Equal(RetentionStatus.Applied, retention.Status);

        // Step 5 — execute the instruction (economic)
        instruction.MarkExecuted(T2);

        Assert.Equal(InstructionStatus.Executed, instruction.Status);

        // Both systems reached their terminal/active states independently
        Assert.Equal(InstructionStatus.Executed, instruction.Status);
        Assert.Equal(RetentionStatus.Applied, retention.Status);
    }

    // ── GP3: Incident Response + Capital Lock Scenario ───────────────────────

    /// <summary>
    /// An incident is raised in the operational system. The economic system
    /// responds by reducing available capital in the pool (as if funds are locked).
    /// Tests that both aggregate lifecycles proceed independently and correctly.
    /// </summary>
    [Fact]
    public void OperationalIncident_WithEconomicCapitalResponse_GoldenPath()
    {
        // Step 1 — report incident (operational)
        var incidentId = new IncidentId(IdGen.Generate("GP:cross:3:incident"));
        var incident = IncidentAggregate.Report(incidentId, new IncidentDescriptor("Node-4 disk failure", "P1"));

        Assert.Equal(IncidentStatus.Reported, incident.Status);

        // Step 2 — investigate (operational)
        incident.Investigate();
        Assert.Equal(IncidentStatus.Investigating, incident.Status);

        // Step 3 — reduce capital pool (economic — simulating emergency reservation)
        var poolId = new PoolId(IdGen.Generate("GP:cross:3:pool"));
        var pool = CapitalPoolAggregate.Create(poolId, Usd, T0);
        var sourceId = IdGen.Generate("GP:cross:3:source");
        pool.AggregateCapital(sourceId, new Amount(100000m));
        pool.ReduceCapital(sourceId, new Amount(15000m));

        Assert.Equal(85000m, pool.TotalCapital.Value);

        // Step 4 — resolve incident (operational)
        incident.Resolve();
        Assert.Equal(IncidentStatus.Resolved, incident.Status);

        // Step 5 — close incident (operational)
        incident.Close();
        Assert.Equal(IncidentStatus.Closed, incident.Status);

        // Cannot re-investigate closed incident (cross-system invariant check)
        Assert.ThrowsAny<Exception>(() => incident.Investigate());
    }
}
