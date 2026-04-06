using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Shared.Contracts.Chain;

/// <summary>
/// Contract for recording policy decisions to WhyceChain as immutable evidence.
/// Implementations live in T0U engine — runtime calls this via DI.
/// </summary>
public interface IChainRecorder
{
    Task<PolicyChainRecord> RecordAsync(
        PolicyEvaluationResult result,
        string correlationId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a policy chain recording operation.
/// </summary>
public sealed record PolicyChainRecord(
    string BlockId,
    string BlockHash,
    string DecisionHash,
    string PreviousHash,
    DateTimeOffset Timestamp);
