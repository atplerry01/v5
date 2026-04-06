using Whyce.Shared.Contracts.Application.Todo;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Runtime;
using Whyce.Shared.Kernel.Domain;

namespace Whyce.Engines.T1M.Steps.Todo;

public sealed class CreateTodoStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;

    public CreateTodoStep(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
    }

    public string Name => "CreateTodo";
    public WorkflowStepType StepType => WorkflowStepType.Command;

    public async Task<WorkflowStepResult> ExecuteAsync(WorkflowExecutionContext context)
    {
        if (context.Payload is not CreateTodoIntent intent)
        {
            return WorkflowStepResult.Failure("Payload is not a valid CreateTodoIntent.");
        }

        var todoId = _idGenerator.Generate($"todo:{intent.UserId}:{intent.Title}");
        var command = new CreateTodoCommand(todoId, intent.Title);

        var route = new DomainRoute("operational", "sandbox", "todo");
        var result = await _dispatcher.DispatchAsync(command, route);

        if (!result.IsSuccess)
        {
            return WorkflowStepResult.Failure(result.Error ?? "Failed to create todo.");
        }

        return WorkflowStepResult.Success(todoId, result.EmittedEvents);
    }
}
