namespace Whyce.Projections.Shared;

/// <summary>
/// Optional interface for projection reducers. Enforces the pure
/// Apply(state, event) → state contract. Reducers that implement this
/// interface MUST remain free of I/O, logging, and non-deterministic calls.
/// </summary>
public interface IProjectionReducer<TState, in TEvent>
{
    TState Apply(TState state, TEvent @event);
}
