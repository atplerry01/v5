using Whyce.Shared.Contracts.Application.Todo;

namespace Whyce.Systems.Midstream.Wss.Workflows;

public sealed class TodoWorkflow
{
    public WorkflowDefinition Build()
    {
        return WorkflowDefinition
            .Create("todo-lifecycle")
            .Step("create", typeof(CreateTodoCommand))
            .Step("update", typeof(UpdateTodoCommand))
            .Step("complete", typeof(CompleteTodoCommand));
    }
}

public sealed class WorkflowDefinition
{
    public string Name { get; private set; } = string.Empty;
    private readonly List<WorkflowStep> _steps = new();
    public IReadOnlyList<WorkflowStep> Steps => _steps.AsReadOnly();

    public static WorkflowDefinition Create(string name)
    {
        return new WorkflowDefinition { Name = name };
    }

    public WorkflowDefinition Step(string name, Type commandType)
    {
        _steps.Add(new WorkflowStep(name, commandType, _steps.Count));
        return this;
    }
}

public sealed record WorkflowStep(string Name, Type CommandType, int Order);
