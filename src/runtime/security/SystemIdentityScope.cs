namespace Whycespace.Runtime.Security;

/// <summary>
/// AsyncLocal-based identity scope for background workers that dispatch
/// commands outside an HTTP request. The runtime's
/// <see cref="Whycespace.Shared.Contracts.Runtime.ICallerIdentityAccessor"/>
/// is canonical HTTP-bound (WP-1 fail-closed) — wrapping a dispatch in a
/// <see cref="SystemIdentityScope"/> tells <c>HttpCallerIdentityAccessor</c>
/// to surface a known system identity instead of throwing.
///
/// The scope is OPT-IN per call site: HTTP requests do NOT enter this scope,
/// so the WP-1 deny-by-default contract for unauthenticated HTTP traffic is
/// preserved. Background workers (WorkflowTriggerWorker, etc.) explicitly
/// declare their system identity via <see cref="Begin"/> immediately before
/// invoking the dispatcher.
///
/// Identity values are short, deterministic system principals
/// (e.g. "system/workflow-trigger") so audit trails can attribute background-
/// initiated commands to a real, declared identity per INV-202 (No Anonymous
/// Execution).
/// </summary>
public sealed class SystemIdentityScope : IDisposable
{
    private static readonly AsyncLocal<SystemIdentityScope?> _current = new();

    public static SystemIdentityScope? Current => _current.Value;

    public string ActorId { get; }
    public string TenantId { get; }
    public IReadOnlyList<string> Roles { get; }

    private readonly SystemIdentityScope? _parent;

    private SystemIdentityScope(string actorId, string tenantId, IReadOnlyList<string> roles)
    {
        if (string.IsNullOrWhiteSpace(actorId))
            throw new ArgumentException("System actorId must be non-empty.", nameof(actorId));
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("System tenantId must be non-empty.", nameof(tenantId));

        ActorId = actorId;
        TenantId = tenantId;
        Roles = roles;
        _parent = _current.Value;
        _current.Value = this;
    }

    public static SystemIdentityScope Begin(string actorId, string tenantId = "system", params string[] roles)
        => new(actorId, tenantId, roles.Length == 0 ? new[] { "system" } : roles);

    public void Dispose() => _current.Value = _parent;
}
