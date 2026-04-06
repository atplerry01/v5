namespace Whycespace.Engines.T3I.Atlas.Service;

/// <summary>
/// T3I engine: workforce optimization business logic.
/// Extracted from Systems.WhyceAtlas.WorkforceOptimizationService.
/// </summary>
public sealed class WorkforceOptimizationEngine
{
    public WorkforceRecommendation Optimize(WorkforceOptimizationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var bestMatch = command.Availability
            .OrderByDescending(a => a.Score)
            .ThenBy(a => a.CurrentLoad)
            .FirstOrDefault();

        return new WorkforceRecommendation
        {
            ClusterId = command.ClusterId,
            TaskType = command.TaskType,
            RecommendedParticipantId = bestMatch?.ParticipantId,
            AvailableCount = command.Availability.Count,
            HasCapacity = bestMatch is not null && bestMatch.CurrentLoad < bestMatch.MaxLoad
        };
    }
}

public sealed record WorkforceOptimizationCommand
{
    public required string ClusterId { get; init; }
    public required string TaskType { get; init; }
    public required IReadOnlyList<WorkforceAvailability> Availability { get; init; }
}

public sealed record WorkforceAvailability
{
    public required string ParticipantId { get; init; }
    public required int CurrentLoad { get; init; }
    public required int MaxLoad { get; init; }
    public required double Score { get; init; }
}

public sealed record WorkforceRecommendation
{
    public required string ClusterId { get; init; }
    public required string TaskType { get; init; }
    public string? RecommendedParticipantId { get; init; }
    public required int AvailableCount { get; init; }
    public required bool HasCapacity { get; init; }
}
