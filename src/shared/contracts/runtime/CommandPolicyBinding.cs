namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// DI-aggregatable binding from a concrete command CLR type to its canonical
/// WHYCEPOLICY action id. Registered once per command at composition time
/// (typically in a per-context module under <c>platform/host/composition</c>),
/// then aggregated via <c>IEnumerable&lt;CommandPolicyBinding&gt;</c> by the
/// <see cref="ICommandPolicyIdRegistry"/> singleton.
///
/// Determinism: the binding is pure data — same Type → same PolicyId across
/// every dispatch, every replay, every host instance.
/// </summary>
public sealed record CommandPolicyBinding(Type CommandType, string PolicyId);
