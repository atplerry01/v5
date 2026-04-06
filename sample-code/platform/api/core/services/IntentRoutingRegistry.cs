namespace Whycespace.Platform.Api.Core.Services;

/// <summary>
/// Central, immutable registry of WorkflowKey → RouteDefinition mappings.
/// Maps each workflow key to its Whycespace cluster taxonomy coordinates:
///   Cluster (Whyce + Name) → Authority → SubCluster
///
/// Deterministic: same WorkflowKey always produces the same route.
/// ExecutionTarget is always "wss" — enforced at the registry level.
///
/// Cluster taxonomy: all cluster names MUST start with "Whyce".
///
/// Version: v1
///
/// MUST NOT:
/// - Call runtime or engines
/// - Evaluate policy
/// - Execute workflows
/// - Contain business logic
/// </summary>
public sealed class IntentRoutingRegistry
{
    public const string ExecutionTargetWss = "wss";
    public const string ClusterPrefix = "Whyce";

    public static readonly IntentRoutingRegistry V1 = new(BuildV1Routes());

    private readonly IReadOnlyDictionary<string, RouteDefinition> _routes;

    public IntentRoutingRegistry(IReadOnlyDictionary<string, RouteDefinition> routes)
    {
        _routes = routes;
    }

    public RouteDefinition? Resolve(string workflowKey)
    {
        if (string.IsNullOrWhiteSpace(workflowKey))
            return null;

        return _routes.GetValueOrDefault(workflowKey);
    }

    public bool IsKnown(string workflowKey)
    {
        if (string.IsNullOrWhiteSpace(workflowKey))
            return false;

        return _routes.ContainsKey(workflowKey);
    }

    public IReadOnlyCollection<string> KnownWorkflowKeys => _routes.Keys.ToList();

    /// <summary>
    /// Validates that a cluster name follows the Whyce taxonomy: "Whyce" + ClusterName.
    /// </summary>
    public static bool IsValidClusterName(string cluster) =>
        !string.IsNullOrWhiteSpace(cluster) && cluster.StartsWith(ClusterPrefix, StringComparison.Ordinal);

    private static Dictionary<string, RouteDefinition> BuildV1Routes()
    {
        var routes = new Dictionary<string, RouteDefinition>(StringComparer.Ordinal);

        // ── WhyceStructure ──────────────────────────────────────────────
        Register(routes, "cluster.spv.create",
            cluster: "WhyceStructure", authority: "cluster", subCluster: "spv", domain: "cluster.spv");

        Register(routes, "cluster.crossspv.execution",
            cluster: "WhyceStructure", authority: "cluster", subCluster: "crossspv", domain: "cluster.crossspv");

        // ── WhyceEconomic ───────────────────────────────────────────────
        Register(routes, "economic.revenue.recognize",
            cluster: "WhyceEconomic", authority: "revenue", subCluster: "recognition", domain: "revenue");

        Register(routes, "economic.capital.wallet.create",
            cluster: "WhyceEconomic", authority: "capital", subCluster: "wallet", domain: "capital.wallet");

        Register(routes, "economic.ledger.transaction.execute",
            cluster: "WhyceEconomic", authority: "ledger", subCluster: "transaction", domain: "ledger.transaction");

        Register(routes, "economic.ledger.accounting.record",
            cluster: "WhyceEconomic", authority: "ledger", subCluster: "accounting", domain: "ledger.accounting");

        Register(routes, "economic.enforcement.apply",
            cluster: "WhyceEconomic", authority: "enforcement", subCluster: "enforcement", domain: "enforcement");

        // ── WhyceGovernance ─────────────────────────────────────────────
        Register(routes, "governance.policy.propose",
            cluster: "WhyceGovernance", authority: "control", subCluster: "policy", domain: "governance.policy");

        Register(routes, "governance.policy.approve",
            cluster: "WhyceGovernance", authority: "control", subCluster: "policy", domain: "governance.policy");

        Register(routes, "governance.policy.activate",
            cluster: "WhyceGovernance", authority: "control", subCluster: "policy", domain: "governance.policy");

        Register(routes, "governance.proposal.create",
            cluster: "WhyceGovernance", authority: "control", subCluster: "proposal", domain: "governance.proposal");

        Register(routes, "governance.vote.cast",
            cluster: "WhyceGovernance", authority: "control", subCluster: "vote", domain: "governance.vote");

        Register(routes, "governance.delegation.delegate",
            cluster: "WhyceGovernance", authority: "control", subCluster: "delegation", domain: "governance.delegation");

        // ── WhyceIdentity ───────────────────────────────────────────────
        Register(routes, "identity.access.register",
            cluster: "WhyceIdentity", authority: "access", subCluster: "identity", domain: "identity.access");

        Register(routes, "identity.access.verify",
            cluster: "WhyceIdentity", authority: "access", subCluster: "identity", domain: "identity.access");

        // ── WhyceOperational ────────────────────────────────────────────
        Register(routes, "operational.todo.create",
            cluster: "WhyceOperational", authority: "sandbox", subCluster: "todo", domain: "operational.todo");

        Register(routes, "operational.incident.report",
            cluster: "WhyceOperational", authority: "global", subCluster: "incident", domain: "operational.incident");

        Register(routes, "operational.incident.resolve",
            cluster: "WhyceOperational", authority: "global", subCluster: "incident", domain: "operational.incident");

        // ── WhyceExecution ──────────────────────────────────────────────
        Register(routes, "execution.workflow.start",
            cluster: "WhyceExecution", authority: "workflow", subCluster: "workflow", domain: "execution.workflow");

        // ── WhycePlatform ───────────────────────────────────────────────
        Register(routes, "platform.ping.ping",
            cluster: "WhycePlatform", authority: "ping", subCluster: "ping", domain: "platform.ping");

        return routes;
    }

    private static void Register(
        Dictionary<string, RouteDefinition> routes,
        string workflowKey,
        string cluster,
        string authority,
        string subCluster,
        string domain)
    {
        routes[workflowKey] = new RouteDefinition
        {
            WorkflowKey = workflowKey,
            Cluster = cluster,
            Authority = authority,
            SubCluster = subCluster,
            Domain = domain,
            ExecutionTarget = ExecutionTargetWss
        };
    }
}

/// <summary>
/// Immutable route definition entry.
/// Maps a workflow key to its full cluster taxonomy coordinates.
/// </summary>
public sealed record RouteDefinition
{
    public required string WorkflowKey { get; init; }
    public required string Cluster { get; init; }
    public required string Authority { get; init; }
    public required string SubCluster { get; init; }
    public required string Domain { get; init; }
    public required string ExecutionTarget { get; init; }
}
