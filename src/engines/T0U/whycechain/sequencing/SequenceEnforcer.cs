namespace Whyce.Engines.T0U.WhyceChain.Sequencing;

/// <summary>
/// Enforces monotonically increasing sequence numbers on chain blocks.
/// Stateless: validates based on inputs only. Chain state is owned by runtime.
/// </summary>
public static class SequenceEnforcer
{
    /// <summary>
    /// Validates that the given sequence is exactly one greater than the last known sequence.
    /// </summary>
    public static (bool IsValid, string? Violation) ValidateSequence(long sequence, long lastKnownSequence)
    {
        if (lastKnownSequence == -1 && sequence == 0)
            return (true, null);

        if (sequence != lastKnownSequence + 1)
        {
            return (false,
                $"Sequence violation: expected {lastKnownSequence + 1}, got {sequence}. " +
                "Chain integrity compromised.");
        }

        return (true, null);
    }
}
