using Whycespace.Shared.Contracts.Systems;
using Whycespace.Shared.Contracts.Systems.Intent;

namespace Whycespace.Systems.Downstream.Platform.Ping;

/// <summary>
/// Downstream process handler for ping diagnostic.
/// Routes to WSS -> Intent -> Runtime -> T4A PingEngine.
/// </summary>
public sealed class PingProcessHandler : IProcessHandler
{
    private readonly IWorkflowRouter _wssRouter;

    public string CommandPrefix => "platform.ping";

    public PingProcessHandler(IWorkflowRouter wssRouter)
    {
        _wssRouter = wssRouter;
    }

    public bool CanHandle(string commandType) =>
        commandType.StartsWith(CommandPrefix, StringComparison.OrdinalIgnoreCase);

    public async Task<IntentResult> HandleAsync(
        ProcessCommand command,
        CancellationToken cancellationToken = default)
    {
        return await _wssRouter.RouteAsync(new WorkflowDispatchRequest
        {
            WorkflowId = "platform.ping",
            CommandType = command.CommandType,
            Payload = command.Payload,
            CorrelationId = command.CorrelationId,
            Cluster = "platform",
            Subcluster = "ping",
            Domain = "platform",
            Context = "ping",
            Timestamp = command.Timestamp,
            AggregateId = command.AggregateId,
            WhyceId = command.WhyceId,
            PolicyId = command.PolicyId
        }, cancellationToken);
    }
}
