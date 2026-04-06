namespace Whycespace.Runtime.Projection.Atlas;

/// <summary>
/// Atlas projection engine — relocated from src/engines/T3I/planning/atlas/.
/// Projection logic belongs in the runtime projection layer, not in engines.
/// </summary>
public sealed class AtlasProjectionEngine
{
    public AtlasProjectionResult Project(AtlasProjectionCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        return new AtlasProjectionResult(command.ProjectionId, true, []);
    }
}

public sealed record AtlasProjectionCommand(string ProjectionId, string QueryExpression);

public sealed record AtlasProjectionResult(string ProjectionId, bool Success, IReadOnlyList<string> Data);
