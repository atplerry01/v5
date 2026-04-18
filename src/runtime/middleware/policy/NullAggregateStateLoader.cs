using Whycespace.Shared.Contracts.Infrastructure.Policy;

namespace Whycespace.Runtime.Middleware.Policy;

/// <summary>
/// Phase 8 B6 — default <see cref="IAggregateStateLoader"/> implementation.
/// Returns <c>null</c> for every request so the middleware stamps
/// <c>input.resource.state = null</c> explicitly when no per-command
/// loader is registered.
///
/// <para>
/// <b>Why explicit null, not silent omission.</b> Rego rules that check
/// <c>input.resource.state.status</c> handle the null branch via a
/// backward-compat guard (<c>not input.resource.state</c>) and allow; the
/// stricter state-aware deny paths never fire. Omitting the field entirely
/// would look identical on the wire but loses the semantic signal that
/// "this command has no loader yet" — future audits / diagnostics would
/// not be able to distinguish "no state available" from "state
/// deliberately empty".
/// </para>
///
/// <para>
/// <b>No behaviour change on fallback.</b> Because rego graceful-degrades
/// when state is null, registering this default in composition keeps
/// allow/deny outcomes identical to pre-B6 — the enrichment seam is
/// purely additive until a domain registers a real loader.
/// </para>
/// </summary>
public sealed class NullAggregateStateLoader : IAggregateStateLoader
{
    public Task<object?> LoadSnapshotAsync(
        Type commandType,
        Guid aggregateId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<object?>(null);
}
