using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Runtime.ControlPlane.Policy;

/// <summary>
/// Runtime orchestrator that links policy activation records to WhyceChain.
/// After activation, records the activation hash to the chain and stores
/// the chain transaction ID back on the activation record.
/// RUNTIME ONLY — engines do not know about this coordination.
/// </summary>
public sealed class PolicyActivationChainLinker
{
    private readonly IChainRecorder _chainRecorder;
    private readonly IPolicyGovernanceRepository _governanceRepository;

    public PolicyActivationChainLinker(
        IChainRecorder chainRecorder,
        IPolicyGovernanceRepository governanceRepository)
    {
        _chainRecorder = chainRecorder ?? throw new ArgumentNullException(nameof(chainRecorder));
        _governanceRepository = governanceRepository ?? throw new ArgumentNullException(nameof(governanceRepository));
    }

    public async Task LinkActivationToChainAsync(
        Guid activationId,
        string activationHash,
        Guid policyId,
        int version,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        // Build a synthetic evaluation result for chain recording
        var chainResult = new PolicyEvaluationResult
        {
            DecisionType = "ACTIVATION",
            IsCompliant = true,
            PolicyIds = [policyId],
            EvaluationTrace = $"activation:v{version}:hash:{activationHash}",
            Source = PolicyExecutionSource.Domain
        };

        var chainRecord = await _chainRecorder.RecordAsync(chainResult, correlationId, cancellationToken);

        // Store chain transaction ID back on the activation record
        await _governanceRepository.UpdateActivationChainTxAsync(
            activationId, chainRecord.BlockId.ToString(), cancellationToken);
    }
}
