using Whycespace.Shared.Contracts.Systems;
using Whycespace.Shared.Contracts.Systems.Intent;

namespace Whycespace.Systems.Downstream.Operational.Sandbox.Todo;

/// <summary>
/// Downstream process handler for todo domain.
/// Interprets business intent and routes to WSS.
/// MUST NOT call runtime or engines directly.
/// </summary>
public sealed class TodoProcessHandler : IProcessHandler
{
    private readonly IWorkflowRouter _wssRouter;

    public string CommandPrefix => "operational.todo";

    public TodoProcessHandler(IWorkflowRouter wssRouter)
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

        return await _wssRouter.RouteAsync(new WorkflowDispatchRequest
        {
            WorkflowId = workflowId,
            CommandType = command.CommandType,
            Payload = command.Payload,
            CorrelationId = command.CorrelationId,
            Cluster = "operational",
            Subcluster = "todo",
            Domain = "operational",
            Context = "todo",
            Timestamp = command.Timestamp,
            AggregateId = command.AggregateId,
            WhyceId = command.WhyceId,
            PolicyId = command.PolicyId
        }, cancellationToken);
    }
}
