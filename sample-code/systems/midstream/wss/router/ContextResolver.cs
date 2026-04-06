namespace Whycespace.Systems.Midstream.Wss.Router;

/// <summary>
/// Resolves cluster, subcluster, and context from an incoming workflow request.
/// Systems layer: composition only — MUST NOT call engines or execute logic.
/// </summary>
public sealed class ContextResolver
{
    public WorkflowContext Resolve(string cluster, string subcluster, string domain, string context)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cluster);
        ArgumentException.ThrowIfNullOrWhiteSpace(subcluster);
        ArgumentException.ThrowIfNullOrWhiteSpace(domain);
        ArgumentException.ThrowIfNullOrWhiteSpace(context);

        return new WorkflowContext(cluster, subcluster, domain, context);
    }
}

public sealed record WorkflowContext(
    string Cluster,
    string Subcluster,
    string Domain,
    string Context);
