namespace Whycespace.Shared.Contracts.Platform.Responses;

/// <summary>
/// Flattened, version-aware response DTO for command execution outcomes.
/// No domain entities exposed — primitives only.
/// </summary>
public sealed record OperationResultDTO
{
    public required int Version { get; init; }
    public required bool Success { get; init; }
    public required string OperationId { get; init; }
    public required DateTimeOffset Timestamp { get; init; }

    public Guid? ResourceId { get; init; }
    public string? ResourceType { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public string? CorrelationId { get; init; }

    public static OperationResultDTO Ok(string operationId, DateTimeOffset timestamp, Guid? resourceId = null, string? resourceType = null) =>
        new()
        {
            Version = 1,
            Success = true,
            OperationId = operationId,
            Timestamp = timestamp,
            ResourceId = resourceId,
            ResourceType = resourceType
        };

    public static OperationResultDTO Fail(string operationId, DateTimeOffset timestamp, string errorCode, string errorMessage) =>
        new()
        {
            Version = 1,
            Success = false,
            OperationId = operationId,
            Timestamp = timestamp,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage
        };
}
