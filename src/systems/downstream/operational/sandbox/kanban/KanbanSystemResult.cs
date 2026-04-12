namespace Whyce.Systems.Downstream.Operational.Sandbox.Kanban;

public sealed record KanbanSystemResult(
    bool Success,
    Guid? BoardId,
    string Status,
    string? Error = null)
{
    public static KanbanSystemResult Ok(Guid boardId, string status = "ok") =>
        new(true, boardId, status);

    public static KanbanSystemResult Fail(string error) =>
        new(false, null, "failed", error);
}
