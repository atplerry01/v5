namespace Whycespace.Runtime.WhyceChain;

/// <summary>
/// Records decision envelopes to WhyceChain for immutable audit trail.
/// All runtime decisions (auth, policy, routing) flow through this interface.
/// </summary>
public interface IDecisionRecorder
{
    Task RecordAsync(DecisionEnvelope decision, CancellationToken cancellationToken = default);
    Task RecordAsync(IEnumerable<DecisionEnvelope> decisions, CancellationToken cancellationToken = default);
}

/// <summary>
/// In-memory implementation for development and testing.
/// Production implementations should persist to WhyceChain via the T0U engine.
/// </summary>
public sealed class InMemoryDecisionRecorder : IDecisionRecorder
{
    private readonly System.Collections.Concurrent.ConcurrentQueue<DecisionEnvelope> _decisions = new();

    public Task RecordAsync(DecisionEnvelope decision, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(decision);
        _decisions.Enqueue(decision);
        return Task.CompletedTask;
    }

    public Task RecordAsync(IEnumerable<DecisionEnvelope> decisions, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(decisions);
        foreach (var decision in decisions)
        {
            _decisions.Enqueue(decision);
        }
        return Task.CompletedTask;
    }

    public IReadOnlyList<DecisionEnvelope> GetAll() => [.. _decisions];
    public int Count => _decisions.Count;
}
