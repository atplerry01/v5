using Whyce.Shared.Contracts.Runtime;
using Whyce.Shared.Kernel.Domain;

namespace Whyce.Systems.Midstream.Wss;

public sealed class WorkflowDispatcher : IWorkflowDispatcher
{
    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public WorkflowDispatcher(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    public async Task<WorkflowResult> StartWorkflowAsync(string workflowName, object payload, DomainRoute route)
    {
        var timestamp = _clock.UtcNow.Ticks.ToString();
        var workflowId = _idGenerator.Generate($"workflow:{workflowName}:{timestamp}");

        var command = new WorkflowStartCommand(workflowId, workflowName, payload);
        var result = await _dispatcher.DispatchAsync(command, route);

        return result.IsSuccess
            ? WorkflowResult.Success(result.EmittedEvents, result.Output)
            : WorkflowResult.Failure(result.Error ?? "Workflow execution failed.");
    }
}
