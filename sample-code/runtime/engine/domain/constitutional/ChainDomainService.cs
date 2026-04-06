using Whycespace.Shared.Primitives.Time;
using Whycespace.Domain.ConstitutionalSystem.Chain.Ledger;
using Whycespace.Runtime.Engine.Domain;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Constitutional.Chain;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Runtime.Engine.Domain.Constitutional;

/// <summary>
/// Runtime implementation of IChainDomainService.
/// Bridges engine layer to domain aggregate creation.
/// </summary>
public sealed class ChainDomainService : GovernedDomainServiceBase, IChainDomainService
{
    public ChainDomainService(
        IPolicyEvaluator policyEvaluator,
        IPolicyDecisionAnchor chainAnchor,
        EnforcementMetrics metrics,
        EnforcementAnomalyEmitter anomalyEmitter,
        IClock clock)
        : base(policyEvaluator, chainAnchor, metrics, anomalyEmitter, clock)
    {
    }

    public async Task<ChainBlockResult> CreateBlockAsync(
        DomainExecutionContext context,
        Guid blockId,
        string previousHash,
        object payload,
        string payloadType,
        string payloadId,
        string correlationId,
        DateTimeOffset timestamp,
        long sequenceNumber)
    {
        var result = await ExecuteGovernedAsync<ChainBlockResult>(context, async () =>
        {
            var domainBlock = ChainBlockAggregate.Create(
                blockId: blockId,
                previousHash: previousHash,
                payload: payload,
                payloadType: payloadType,
                payloadId: payloadId,
                correlationId: correlationId,
                timestamp: timestamp,
                sequenceNumber: sequenceNumber);

            return new ChainBlockResult(
                domainBlock.Id,
                domainBlock.CurrentHash,
                domainBlock.PayloadHash,
                domainBlock.PreviousHash);
        });

        return result.Data!;
    }
}
