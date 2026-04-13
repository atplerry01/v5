using Whyce.Shared.Contracts.Events.Operational.Sandbox.Todo;
using Whyce.Shared.Contracts.Operational.Sandbox.Todo;

namespace Whyce.Projections.Operational.Sandbox.Todo.Reducer;

/// <summary>
/// Pure state reducer for the Todo read model.
/// Each Apply method takes the current state + an event and returns the new state.
/// No I/O, no side effects — transformation logic only.
/// </summary>
public static class TodoProjectionReducer
{
    public static TodoReadModel Apply(TodoReadModel state, TodoCreatedEventSchema e)
    {
        return state with { Title = e.Title, IsCompleted = false, Status = "active" };
    }

    public static TodoReadModel Apply(TodoReadModel state, TodoUpdatedEventSchema e)
    {
        return state with { Title = e.Title };
    }

    public static TodoReadModel Apply(TodoReadModel state, TodoCompletedEventSchema _)
    {
        return state with { IsCompleted = true, Status = "completed" };
    }
}
