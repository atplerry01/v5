namespace Whycespace.Engines.T4A.Tools;

public sealed class AdminToolEngine
{
    public AdminToolResult Execute(AdminToolCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        return new AdminToolResult(command.ToolName, true, string.Empty);
    }
}

public sealed record AdminToolCommand(string ToolName, string Action, IReadOnlyList<string> Parameters);

public sealed record AdminToolResult(string ToolName, bool Success, string Output);
