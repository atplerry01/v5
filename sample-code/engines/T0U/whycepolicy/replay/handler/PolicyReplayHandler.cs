using Whycespace.Shared.Contracts.Policy;
using Whycespace.Shared.Utils;

namespace Whycespace.Engines.T0U.WhycePolicy.Replay;

/// <summary>
/// Re-evaluates a policy decision at a specific point in time.
/// Stateless — reads historical versions from IPolicyReadModel (projections).
/// Deterministic: same input → same result → same hash.
/// </summary>
public sealed class PolicyReplayEngine
{
    private readonly IPolicyEvaluator _evaluator;
    private readonly IPolicyReadModel _readModel;

    public PolicyReplayEngine(
        IPolicyEvaluator evaluator,
        IPolicyReadModel readModel)
    {
        _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        _readModel = readModel ?? throw new ArgumentNullException(nameof(readModel));
    }

    public async Task<PolicyReplayResult> ReplayAsync(
        PolicyEvaluationInput input,
        string originalDecisionHash,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentException.ThrowIfNullOrWhiteSpace(originalDecisionHash);

        var historicalVersion = input.PolicyId.HasValue
            ? await _readModel.GetVersionByTimeAsync(
                input.PolicyId.Value, input.Timestamp, cancellationToken)
            : null;

        var result = await _evaluator.EvaluateAsync(input, cancellationToken);

        var replayHash = PolicyHashGenerator.ComputeDecisionHash(
            result.PolicyIds,
            result.DecisionType,
            result.EvaluationTrace);

        var matches = string.Equals(replayHash, originalDecisionHash, StringComparison.Ordinal);

        return new PolicyReplayResult(result, replayHash, originalDecisionHash, matches, historicalVersion);
    }
}

public sealed record PolicyReplayResult(
    PolicyEvaluationResult Result,
    string ReplayHash,
    string OriginalHash,
    bool HashMatches,
    PolicyVersionRecord? HistoricalVersion);
