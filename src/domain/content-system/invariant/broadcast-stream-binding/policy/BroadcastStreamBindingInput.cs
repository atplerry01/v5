namespace Whycespace.Domain.ContentSystem.Invariant.BroadcastStreamBinding;

/// <summary>
/// Minimum fact-set required to evaluate BroadcastStreamBindingPolicy.
/// Assembled by the invoking handler from the Stream projection + command.
/// Pure record — no aggregate references, no VO imports (to keep the
/// invariant pure per D-PURITY-01).
/// </summary>
public readonly record struct BroadcastStreamBindingInput(
    Guid StreamId,
    bool StreamExists,
    bool StreamStatusIsTerminal);
