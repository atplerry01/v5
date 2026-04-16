namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// Resolves the WHYCEPOLICY action id for a given command CLR type.
/// Consulted by <c>SystemIntentDispatcher</c> when constructing the
/// <see cref="CommandContext"/> so the downstream <c>PolicyMiddleware</c>
/// evaluates the canonical per-command policy rather than a generic default.
///
/// Resolution must be deterministic and stateless — same Type always yields
/// the same id. Unmapped commands fall back to an implementation-defined
/// default (currently <c>"whyce-policy-default"</c>) so existing untyped
/// flows (Todo, Kanban, etc.) continue to evaluate against the project-wide
/// fallback policy until their own bindings are added.
/// </summary>
public interface ICommandPolicyIdRegistry
{
    string Resolve(Type commandType);
}
