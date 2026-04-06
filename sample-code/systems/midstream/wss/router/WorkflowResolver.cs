namespace Whycespace.Systems.Midstream.Wss.Router;

/// <summary>
/// Resolves a workflow definition by ID from the registered workflow definitions.
/// Systems layer: composition only — MUST NOT call engines or execute logic.
/// </summary>
public sealed class WorkflowResolver
{
    private readonly Dictionary<string, WorkflowRegistration> _registrations = new();

    public void Register(string workflowId, string cluster, string subcluster, string domain, IReadOnlyList<string> steps)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);
        _registrations[workflowId] = new WorkflowRegistration(workflowId, cluster, subcluster, domain, steps);
    }

    public WorkflowRegistration Resolve(string workflowId)
    {
        if (!_registrations.TryGetValue(workflowId, out var registration))
            throw new InvalidOperationException($"Workflow '{workflowId}' is not registered.");

        return registration;
    }

    public bool HasWorkflow(string workflowId) => _registrations.ContainsKey(workflowId);
}

public sealed record WorkflowRegistration(
    string WorkflowId,
    string Cluster,
    string Subcluster,
    string Domain,
    IReadOnlyList<string> Steps);
