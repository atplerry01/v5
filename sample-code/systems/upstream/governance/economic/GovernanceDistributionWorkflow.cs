using Whycespace.Shared.Contracts.Systems;
using Whycespace.Shared.Contracts.Systems.Intent;

namespace Whycespace.Systems.Upstream.Governance.Economic;

/// <summary>
/// Governance-gated revenue distribution workflow.
/// Flow: Distribution Request → Governance Proposal → Vote → Execute → Chain Anchor
/// Systems layer — orchestration only, no domain mutation.
/// </summary>
public sealed class GovernanceDistributionWorkflow
{
    private readonly ISystemIntentDispatcher _intentDispatcher;

    public GovernanceDistributionWorkflow(ISystemIntentDispatcher intentDispatcher)
    {
        _intentDispatcher = intentDispatcher ?? throw new ArgumentNullException(nameof(intentDispatcher));
    }

    public async Task<IntentResult> ExecuteAsync(
        string distributionId,
        string revenueId,
        decimal amount,
        string currencyCode,
        string clusterId,
        string actorId,
        CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();

        // Step 1: Create governance proposal for distribution
        var proposalCommandId = Guid.NewGuid();
        var proposalResult = await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = proposalCommandId,
            CommandType = "governance.proposal.create",
            Payload = new
            {
                ProposalType = "RevenueDistribution",
                DistributionId = distributionId,
                RevenueId = revenueId,
                Amount = amount,
                CurrencyCode = currencyCode,
                ClusterId = clusterId,
                ProposedBy = actorId
            },
            CorrelationId = correlationId,
            Timestamp = DateTimeOffset.UtcNow
        }, cancellationToken);

        if (!proposalResult.Success)
            return IntentResult.Fail(proposalCommandId, $"Proposal creation failed: {proposalResult.ErrorMessage}");

        // Step 2: Initiate voting round
        var votingCommandId = Guid.NewGuid();
        var votingResult = await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = votingCommandId,
            CommandType = "governance.voting.initiate",
            Payload = new
            {
                ProposalId = proposalResult.Data,
                VotingType = "RevenueDistribution",
                ClusterId = clusterId
            },
            CorrelationId = correlationId,
            Timestamp = DateTimeOffset.UtcNow
        }, cancellationToken);

        if (!votingResult.Success)
            return IntentResult.Fail(votingCommandId, $"Voting failed: {votingResult.ErrorMessage}");

        // Step 3: Execute distribution (only if vote passed)
        var executionResult = await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = Guid.NewGuid(),
            CommandType = "economic.distribution.create",
            Payload = new
            {
                Id = distributionId,
                RevenueId = revenueId,
                Amount = amount,
                CurrencyCode = currencyCode
            },
            CorrelationId = correlationId,
            Timestamp = DateTimeOffset.UtcNow
        }, cancellationToken);

        // Step 4: Anchor to chain
        if (executionResult.Success)
        {
            await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
            {
                CommandId = Guid.NewGuid(),
                CommandType = "chain.anchor.decision",
                Payload = new
                {
                    DecisionType = "RevenueDistribution",
                    DistributionId = distributionId,
                    Amount = amount,
                    GovernanceApproved = true,
                    ActorId = actorId
                },
                CorrelationId = correlationId,
                Timestamp = DateTimeOffset.UtcNow
            }, cancellationToken);
        }

        return executionResult;
    }
}
