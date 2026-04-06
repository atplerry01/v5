using System.Text.Json;

namespace Whycespace.Runtime.Workflow.State;

/// <summary>
/// EF Core entity for persistent workflow state storage.
/// Steps are serialized as JSON for simplicity and schema flexibility.
/// </summary>
public sealed class WorkflowStateEntity
{
    public Guid WorkflowId { get; set; }
    public string CommandType { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string ExecutionId { get; set; } = string.Empty;
    public int Status { get; set; }
    public int CurrentStepIndex { get; set; }
    public int TotalSteps { get; set; }
    public long Version { get; set; }
    public string StepsJson { get; set; } = "[]";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset LastModifiedAt { get; set; }
}

public static class WorkflowStateMapping
{
    public static WorkflowStateEntity ToEntity(this WorkflowStateSnapshot snapshot)
    {
        return new WorkflowStateEntity
        {
            WorkflowId = snapshot.WorkflowId,
            CommandType = snapshot.CommandType,
            CorrelationId = snapshot.CorrelationId,
            ExecutionId = snapshot.ExecutionId,
            Status = (int)snapshot.Status,
            CurrentStepIndex = snapshot.CurrentStepIndex,
            TotalSteps = snapshot.TotalSteps,
            Version = snapshot.Version,
            StepsJson = JsonSerializer.Serialize(snapshot.Steps),
            CreatedAt = snapshot.CreatedAt,
            CompletedAt = snapshot.CompletedAt,
            ErrorMessage = snapshot.ErrorMessage,
            LastModifiedAt = snapshot.LastModifiedAt
        };
    }

    public static WorkflowStateSnapshot ToDomain(this WorkflowStateEntity entity)
    {
        return new WorkflowStateSnapshot
        {
            WorkflowId = entity.WorkflowId,
            CommandType = entity.CommandType,
            CorrelationId = entity.CorrelationId,
            ExecutionId = entity.ExecutionId,
            Status = (WorkflowStatus)entity.Status,
            CurrentStepIndex = entity.CurrentStepIndex,
            TotalSteps = entity.TotalSteps,
            Version = entity.Version,
            Steps = JsonSerializer.Deserialize<List<StepSnapshot>>(entity.StepsJson) ?? new List<StepSnapshot>(),
            CreatedAt = entity.CreatedAt,
            CompletedAt = entity.CompletedAt,
            ErrorMessage = entity.ErrorMessage,
            LastModifiedAt = entity.LastModifiedAt
        };
    }

    public static void UpdateFrom(this WorkflowStateEntity entity, WorkflowStateSnapshot snapshot)
    {
        entity.CommandType = snapshot.CommandType;
        entity.CorrelationId = snapshot.CorrelationId;
        entity.ExecutionId = snapshot.ExecutionId;
        entity.Status = (int)snapshot.Status;
        entity.CurrentStepIndex = snapshot.CurrentStepIndex;
        entity.TotalSteps = snapshot.TotalSteps;
        entity.Version = snapshot.Version;
        entity.StepsJson = JsonSerializer.Serialize(snapshot.Steps);
        entity.CompletedAt = snapshot.CompletedAt;
        entity.ErrorMessage = snapshot.ErrorMessage;
        entity.LastModifiedAt = snapshot.LastModifiedAt;
    }
}
