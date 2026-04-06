namespace Whycespace.Systems.Midstream.Wss.Workflows.Governance.Policy;

/// <summary>
/// Resolves the correct workflow version for a given workflow ID.
/// Supports future version upgrades without breaking in-flight executions.
/// STRICTLY DECLARATIVE — no business logic, no side effects.
/// </summary>
public sealed class WorkflowVersionResolver
{
    private readonly Dictionary<string, WorkflowVersionEntry> _versions = new(StringComparer.OrdinalIgnoreCase);

    public WorkflowVersionResolver()
    {
        RegisterDefaults();
    }

    /// <summary>
    /// Resolves the active workflow version for a given workflow ID.
    /// Returns the latest registered version.
    /// </summary>
    public WorkflowVersionEntry Resolve(string workflowId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowId);

        if (_versions.TryGetValue(workflowId, out var entry))
            return entry;

        // Default: version 1 for unregistered workflows
        return new WorkflowVersionEntry
        {
            WorkflowId = workflowId,
            Version = "v1",
            IsActive = true
        };
    }

    /// <summary>
    /// Registers a workflow version. Overwrites any existing registration for the same workflow ID.
    /// </summary>
    public void Register(string workflowId, string version)
    {
        _versions[workflowId] = new WorkflowVersionEntry
        {
            WorkflowId = workflowId,
            Version = version,
            IsActive = true
        };
    }

    private void RegisterDefaults()
    {
        foreach (var action in PolicyWorkflowDefinition.Actions)
        {
            var workflowId = $"{PolicyWorkflowDefinition.WorkflowPrefix}.{action}";
            Register(workflowId, PolicyWorkflowDefinition.WorkflowVersion);
        }
    }
}

public sealed record WorkflowVersionEntry
{
    public required string WorkflowId { get; init; }
    public required string Version { get; init; }
    public required bool IsActive { get; init; }
}
