namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// Optional marker for commands that name an authoritative SPV. When a
/// command implements <see cref="IHsidCommand"/>, the runtime control-plane
/// HSID prelude resolves topology via <c>ITopologyResolver</c> using the
/// <see cref="SpvId"/> rather than deriving topology from
/// classification/context/domain. Commands that do NOT implement this
/// interface fall back to deterministic SHA256 derivation — also stable but
/// not authoritative.
///
/// Guard reference: deterministic-id.guard.md G14, G17.
/// </summary>
public interface IHsidCommand
{
    string SpvId { get; }
}
