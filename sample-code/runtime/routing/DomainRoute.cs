namespace Whycespace.Runtime.Routing;

/// <summary>
/// Represents a fully-qualified domain route.
/// All runtime routing MUST resolve through this model — no hardcoded paths.
///
/// Supports both deep and shallow domain paths:
///   Global:  operational.incident          (Cluster + Context only)
///   Scoped:  operational.todo (sandbox context)
/// </summary>
public sealed record DomainRoute
{
    public required string Cluster { get; init; }
    public string? SubCluster { get; init; }
    public string? App { get; init; }
    public required string Context { get; init; }

    /// <summary>
    /// Fully-qualified domain path built from non-null segments.
    /// e.g. "operational.incident" or "operational.todo"
    /// </summary>
    public string QualifiedPath
    {
        get
        {
            var segments = new List<string>(4) { Cluster };
            if (!string.IsNullOrEmpty(SubCluster)) segments.Add(SubCluster);
            if (!string.IsNullOrEmpty(App)) segments.Add(App);
            segments.Add(Context);
            return string.Join('.', segments);
        }
    }

    /// <summary>
    /// Aggregate type derived from domain path (used for event routing).
    /// </summary>
    public string AggregateType => QualifiedPath;

    /// <summary>
    /// Resolves engine command type for a given action.
    /// e.g. action="create" → "operational.incident.create"
    /// </summary>
    public string ResolveCommandType(string action) => $"{QualifiedPath}.{action}";

    /// <summary>
    /// Resolves event type for a given action and outcome.
    /// e.g. action="create", outcome="Completed" → "operational.incident.create.Completed"
    /// </summary>
    public string ResolveEventType(string action, string outcome) => $"{QualifiedPath}.{action}.{outcome}";

    /// <summary>
    /// Resolves the projection name for this domain context.
    /// </summary>
    public string ProjectionName => QualifiedPath;

    /// <summary>
    /// Resolves the Kafka domain topic for a given action.
    /// e.g. "whyce.operational.incident.create"
    /// </summary>
    public string ResolveKafkaTopic(string action) => $"whyce.{QualifiedPath}.{action}";

    public override string ToString() => QualifiedPath;
}
