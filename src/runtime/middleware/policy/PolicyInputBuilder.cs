using System.Text.Json;
using Whycespace.Shared.Contracts.Infrastructure.Policy;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Runtime.Middleware.Policy;

/// <summary>
/// Phase 8 B6 — pure, deterministic, no-IO builder that stamps the
/// <c>Command</c> / <c>ResourceState</c> / <c>Now</c> / <c>AggregateId</c>
/// init-only properties on a <see cref="PolicyContext"/>. Lives inside
/// <c>PolicyMiddleware</c>'s composition so the middleware owns the
/// enrichment decision and the OPA adapter remains a pure transport layer.
///
/// <para>
/// <b>Pure function contract.</b> Given identical inputs, this builder
/// produces an identical <see cref="PolicyContext"/> — no clock reads,
/// no event-store calls, no dictionary-iteration order leaks.
/// <c>cmd.GetType()</c>-keyed type lookups are the only reflection used.
/// </para>
///
/// <para>
/// <b>Command shape stability.</b> The command object is attached by
/// reference; the OPA adapter serialises it with the shared
/// <see cref="SerializerOptions"/> at the wire boundary. No rewriting,
/// renaming, or projection happens here — the whole point is that rego
/// sees exactly what the engine will receive, byte-for-byte (modulo the
/// snake-case JSON naming policy).
/// </para>
/// </summary>
public static class PolicyInputBuilder
{
    /// <summary>
    /// Phase 8 B6 — canonical JSON naming for OPA input. Existing rego
    /// files already reference snake-case keys
    /// (<c>input.command.counterparty_id</c>,
    /// <c>input.resource.state.status</c>), so this is the only naming
    /// that keeps the enriched payload compatible with every already-
    /// shipped policy file. The options instance is cached as a static
    /// singleton so every serialisation across the pipeline uses
    /// byte-identical settings — a single source of truth for input
    /// shape determinism.
    /// </summary>
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
    };

    /// <summary>
    /// Returns a new <see cref="PolicyContext"/> with the B6 enrichment
    /// properties stamped. The pre-B6 positional fields are forwarded
    /// verbatim so backward-compat callers are unaffected.
    /// </summary>
    public static PolicyContext Enrich(
        PolicyContext baseContext,
        object command,
        object? resourceState,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(baseContext);
        ArgumentNullException.ThrowIfNull(command);

        return baseContext with
        {
            Command = command,
            ResourceState = resourceState,
            Now = now,
            AggregateId = TryResolveAggregateId(command)
        };
    }

    /// <summary>
    /// R1 §6 — variant that additionally stamps <c>input.environment</c> and
    /// <c>input.jurisdiction</c>. Callers supply strings resolved from host
    /// configuration (environment) and command / tenant metadata (jurisdiction).
    /// <c>null</c> on either passes through and causes rego to omit the field,
    /// preserving backward-compat with policies that don't consume the overlay.
    /// </summary>
    public static PolicyContext Enrich(
        PolicyContext baseContext,
        object command,
        object? resourceState,
        DateTimeOffset now,
        string? environment,
        string? jurisdiction)
    {
        ArgumentNullException.ThrowIfNull(baseContext);
        ArgumentNullException.ThrowIfNull(command);

        return baseContext with
        {
            Command = command,
            ResourceState = resourceState,
            Now = now,
            AggregateId = TryResolveAggregateId(command),
            Environment = environment,
            Jurisdiction = jurisdiction
        };
    }

    /// <summary>
    /// Lifts the aggregate id off the command when it implements
    /// <see cref="IHasAggregateId"/>. Returns <c>null</c> for commands
    /// without a stream anchor (workflow-level commands, etc.) so the
    /// OPA payload explicitly omits the field rather than stamping
    /// <see cref="Guid.Empty"/> which rego could mis-read as a real id.
    /// </summary>
    internal static Guid? TryResolveAggregateId(object command) =>
        command is IHasAggregateId tagged ? tagged.AggregateId : null;
}
