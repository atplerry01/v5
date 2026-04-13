namespace Whycespace.Systems.Downstream.Operational.Sandbox.Todo;

public sealed record TodoSystemResult(
    bool Success,
    Guid? TodoId,
    string Status,
    string? Error = null)
{
    public static TodoSystemResult Ok(Guid todoId) =>
        new(true, todoId, "created");

    public static TodoSystemResult Fail(string error) =>
        new(false, null, "failed", error);
}
