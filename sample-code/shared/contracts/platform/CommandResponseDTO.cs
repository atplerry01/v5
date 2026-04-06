namespace Whycespace.Shared.Contracts.Platform;

/// <summary>
/// Generic command response envelope returned by the Platform API after command execution.
/// Captures the outcome of Command → Workflow → Response flow.
/// No domain entities exposed — primitives only.
/// </summary>
public sealed record CommandResponseDTO
{
    public required string CommandId { get; init; }
    public required string CommandType { get; init; }
    public required int Version { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public required bool Success { get; init; }

    public Guid? ResourceId { get; init; }
    public string? ResourceType { get; init; }
    public string? WorkflowInstanceId { get; init; }
    public string? WorkflowState { get; init; }
    public string? CorrelationId { get; init; }

    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public IReadOnlyList<string>? ValidationErrors { get; init; }

    public static CommandResponseDTO Ok(
        string commandId,
        string commandType,
        DateTimeOffset timestamp,
        Guid? resourceId = null,
        string? resourceType = null,
        string? workflowInstanceId = null,
        string? correlationId = null) =>
        new()
        {
            CommandId = commandId,
            CommandType = commandType,
            Version = 1,
            Timestamp = timestamp,
            Success = true,
            ResourceId = resourceId,
            ResourceType = resourceType,
            WorkflowInstanceId = workflowInstanceId,
            CorrelationId = correlationId
        };

    public static CommandResponseDTO Fail(
        string commandId,
        string commandType,
        DateTimeOffset timestamp,
        string errorCode,
        string errorMessage,
        IReadOnlyList<string>? validationErrors = null,
        string? correlationId = null) =>
        new()
        {
            CommandId = commandId,
            CommandType = commandType,
            Version = 1,
            Timestamp = timestamp,
            Success = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            ValidationErrors = validationErrors,
            CorrelationId = correlationId
        };
}
