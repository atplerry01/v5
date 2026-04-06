using Whycespace.Shared.Contracts.Systems;
using Whycespace.Shared.Contracts.Systems.Intent;

namespace Whycespace.Systems.Downstream.Policy;

/// <summary>
/// Downstream process handler for policy governance domain.
/// Interprets governance intent and routes to WSS.
/// MUST NOT call runtime or engines directly.
///
/// Commands:
///   governance.policy.proposal.submit
///   governance.policy.approve
///   governance.policy.activate
/// </summary>
public sealed class PolicyProcessHandler : IProcessHandler
{
    private readonly IWorkflowRouter _wssRouter;

    public string CommandPrefix => "governance.policy";

    public PolicyProcessHandler(IWorkflowRouter wssRouter)
    {
        _wssRouter = wssRouter;
    }

    public bool CanHandle(string commandType) =>
        commandType.StartsWith(CommandPrefix, StringComparison.OrdinalIgnoreCase);

    public async Task<IntentResult> HandleAsync(
        ProcessCommand command,
        CancellationToken cancellationToken = default)
    {
        var action = command.CommandType[(CommandPrefix.Length + 1)..];
        var workflowId = $"{CommandPrefix}.{action}";

        // Attach governance traceability metadata based on command action
        var governanceMetadata = BuildGovernanceMetadata(action, command);

        return await _wssRouter.RouteAsync(new WorkflowDispatchRequest
        {
            WorkflowId = workflowId,
            CommandType = command.CommandType,
            Payload = command.Payload,
            CorrelationId = command.CorrelationId,
            Cluster = "governance",
            Subcluster = "policy",
            Domain = "governance",
            Context = "policy",
            Timestamp = command.Timestamp,
            AggregateId = command.AggregateId,
            WhyceId = command.WhyceId,
            PolicyId = command.PolicyId,
            GovernanceMetadata = governanceMetadata
        }, cancellationToken);
    }

    /// <summary>
    /// Extracts governance actor metadata from command payload.
    /// Ensures proposed_by, approved_by, activated_by flow through
    /// to runtime and WhyceChain for traceability.
    /// </summary>
    private static GovernanceMetadata BuildGovernanceMetadata(string action, ProcessCommand command)
    {
        var actorId = command.WhyceId;
        var timestamp = command.Timestamp;

        return action switch
        {
            "proposal.submit" => new GovernanceMetadata
            {
                Action = GovernanceAction.Propose,
                ActorId = actorId,
                Timestamp = timestamp,
                ProposedBy = actorId
            },
            "approve" => new GovernanceMetadata
            {
                Action = GovernanceAction.Approve,
                ActorId = actorId,
                Timestamp = timestamp,
                ApprovedBy = actorId
            },
            "activate" => new GovernanceMetadata
            {
                Action = GovernanceAction.Activate,
                ActorId = actorId,
                Timestamp = timestamp,
                ActivatedBy = actorId
            },
            _ => new GovernanceMetadata
            {
                Action = GovernanceAction.Unknown,
                ActorId = actorId,
                Timestamp = timestamp
            }
        };
    }
}
