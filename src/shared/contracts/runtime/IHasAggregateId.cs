namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// Opt-in contract a command record may implement to expose its aggregate id
/// without relying on reflection-by-property-name. Preferred over name-based
/// lookup; the dispatcher checks for this interface first and only falls back
/// to a canonical property-name list when it is not implemented.
///
/// Determinism: the implementation must return the same value for the same
/// command instance on every invocation — typically a record's init-only
/// property, which satisfies this trivially.
/// </summary>
public interface IHasAggregateId
{
    Guid AggregateId { get; }
}
