using Whycespace.Platform.Api.Core.Contracts;

namespace Whycespace.Platform.Api.Core.Services;

/// <summary>
/// Central, immutable registry of intent type → classification mappings.
/// Deterministic: same IntentType always produces the same ClassifiedIntent coordinates.
/// No business logic — pure structural mapping from user-facing intent names
/// to system routing coordinates (classification, domain, workflow key).
///
/// Version: v1
///
/// MUST NOT:
/// - Call runtime or engines
/// - Evaluate policy
/// - Execute workflows
/// - Contain business logic
/// </summary>
public sealed class IntentMappingRegistry
{
    public static readonly IntentMappingRegistry V1 = new(BuildV1Mappings());

    private readonly IReadOnlyDictionary<string, IntentMapping> _mappings;

    public IntentMappingRegistry(IReadOnlyDictionary<string, IntentMapping> mappings)
    {
        _mappings = mappings;
    }

    public IntentMapping? Resolve(string intentType)
    {
        if (string.IsNullOrWhiteSpace(intentType))
            return null;

        var normalized = intentType.Trim().ToLowerInvariant();
        return _mappings.GetValueOrDefault(normalized);
    }

    public bool IsKnown(string intentType)
    {
        if (string.IsNullOrWhiteSpace(intentType))
            return false;

        return _mappings.ContainsKey(intentType.Trim().ToLowerInvariant());
    }

    public IReadOnlyCollection<string> KnownIntentTypes => _mappings.Keys.ToList();

    private static Dictionary<string, IntentMapping> BuildV1Mappings()
    {
        var mappings = new Dictionary<string, IntentMapping>(StringComparer.OrdinalIgnoreCase);

        // ── Cluster / SPV ───────────────────────────────────────────────
        Register(mappings, "create_spv",
            classification: "clusters", domain: "cluster.spv",
            workflowKey: "cluster.spv.create",
            cluster: "structural", subcluster: "cluster", context: "spv");

        Register(mappings, "execute_cross_spv",
            classification: "clusters", domain: "cluster.crossspv",
            workflowKey: "cluster.crossspv.execution",
            cluster: "structural", subcluster: "cluster", context: "crossspv");

        // ── Economic ────────────────────────────────────────────────────
        Register(mappings, "recognize_revenue",
            classification: "economic", domain: "revenue",
            workflowKey: "economic.revenue.recognize",
            cluster: "economic", subcluster: "capital", context: "revenue");

        Register(mappings, "create_wallet",
            classification: "economic", domain: "capital.wallet",
            workflowKey: "economic.capital.wallet.create",
            cluster: "economic", subcluster: "capital", context: "wallet");

        Register(mappings, "execute_transaction",
            classification: "economic", domain: "ledger.transaction",
            workflowKey: "economic.ledger.transaction.execute",
            cluster: "economic", subcluster: "ledger", context: "transaction");

        Register(mappings, "record_accounting_entry",
            classification: "economic", domain: "ledger.accounting",
            workflowKey: "economic.ledger.accounting.record",
            cluster: "economic", subcluster: "ledger", context: "accounting");

        Register(mappings, "enforce_economic_rule",
            classification: "economic", domain: "enforcement",
            workflowKey: "economic.enforcement.apply",
            cluster: "economic", subcluster: "enforcement", context: "enforcement");

        // ── Governance ──────────────────────────────────────────────────
        Register(mappings, "propose_policy",
            classification: "governance", domain: "governance.policy",
            workflowKey: "governance.policy.propose",
            cluster: "governance", subcluster: "control", context: "policy");

        Register(mappings, "approve_policy",
            classification: "governance", domain: "governance.policy",
            workflowKey: "governance.policy.approve",
            cluster: "governance", subcluster: "control", context: "policy");

        Register(mappings, "activate_policy",
            classification: "governance", domain: "governance.policy",
            workflowKey: "governance.policy.activate",
            cluster: "governance", subcluster: "control", context: "policy");

        Register(mappings, "create_proposal",
            classification: "governance", domain: "governance.proposal",
            workflowKey: "governance.proposal.create",
            cluster: "governance", subcluster: "control", context: "proposal");

        Register(mappings, "cast_vote",
            classification: "governance", domain: "governance.vote",
            workflowKey: "governance.vote.cast",
            cluster: "governance", subcluster: "control", context: "vote");

        Register(mappings, "delegate_authority",
            classification: "governance", domain: "governance.delegation",
            workflowKey: "governance.delegation.delegate",
            cluster: "governance", subcluster: "control", context: "delegation");

        // ── Identity ────────────────────────────────────────────────────
        Register(mappings, "register_identity",
            classification: "identity", domain: "identity.access",
            workflowKey: "identity.access.register",
            cluster: "identity", subcluster: "access", context: "identity");

        Register(mappings, "verify_identity",
            classification: "identity", domain: "identity.access",
            workflowKey: "identity.access.verify",
            cluster: "identity", subcluster: "access", context: "identity");

        // ── Operational ─────────────────────────────────────────────────
        Register(mappings, "create_todo",
            classification: "operational", domain: "operational.todo",
            workflowKey: "operational.todo.create",
            cluster: "operational", subcluster: "sandbox", context: "todo");

        Register(mappings, "report_incident",
            classification: "operational", domain: "operational.incident",
            workflowKey: "operational.incident.report",
            cluster: "operational", subcluster: "global", context: "incident");

        Register(mappings, "resolve_incident",
            classification: "operational", domain: "operational.incident",
            workflowKey: "operational.incident.resolve",
            cluster: "operational", subcluster: "global", context: "incident");

        // ── Workflow / Execution ────────────────────────────────────────
        Register(mappings, "start_workflow",
            classification: "workflow", domain: "execution.workflow",
            workflowKey: "execution.workflow.start",
            cluster: "execution", subcluster: "workflow", context: "workflow");

        // ── Platform ────────────────────────────────────────────────────
        Register(mappings, "ping",
            classification: "platform", domain: "platform.ping",
            workflowKey: "platform.ping.ping",
            cluster: "platform", subcluster: "ping", context: "ping");

        return mappings;
    }

    private static void Register(
        Dictionary<string, IntentMapping> mappings,
        string intentType,
        string classification,
        string domain,
        string workflowKey,
        string cluster,
        string subcluster,
        string context)
    {
        mappings[intentType.ToLowerInvariant()] = new IntentMapping
        {
            IntentType = intentType,
            Classification = classification,
            Domain = domain,
            WorkflowKey = workflowKey,
            Cluster = cluster,
            Subcluster = subcluster,
            Context = context
        };
    }
}

/// <summary>
/// Immutable mapping entry from an intent type to system routing coordinates.
/// </summary>
public sealed record IntentMapping
{
    public required string IntentType { get; init; }
    public required string Classification { get; init; }
    public required string Domain { get; init; }
    public required string WorkflowKey { get; init; }
    public required string Cluster { get; init; }
    public required string Subcluster { get; init; }
    public required string Context { get; init; }
}
