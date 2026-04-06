namespace Whycespace.Shared.Contracts.Platform;

/// <summary>
/// Generic query response envelope returned by the Platform API for read operations.
/// No domain entities exposed — primitives only.
/// </summary>
public sealed record QueryResponseDTO
{
    public required string QueryId { get; init; }
    public required string QueryType { get; init; }
    public required int Version { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public required bool Success { get; init; }

    /// <summary>Serialized result payload — projection data, never a domain entity.</summary>
    public string? Payload { get; init; }

    public int? TotalCount { get; init; }
    public int? PageIndex { get; init; }
    public int? PageSize { get; init; }
    public string? CorrelationId { get; init; }

    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }

    public static QueryResponseDTO Ok(
        string queryId,
        string queryType,
        DateTimeOffset timestamp,
        string payload,
        int? totalCount = null,
        int? pageIndex = null,
        int? pageSize = null,
        string? correlationId = null) =>
        new()
        {
            QueryId = queryId,
            QueryType = queryType,
            Version = 1,
            Timestamp = timestamp,
            Success = true,
            Payload = payload,
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = pageSize,
            CorrelationId = correlationId
        };

    public static QueryResponseDTO Fail(
        string queryId,
        string queryType,
        DateTimeOffset timestamp,
        string errorCode,
        string errorMessage,
        string? correlationId = null) =>
        new()
        {
            QueryId = queryId,
            QueryType = queryType,
            Version = 1,
            Timestamp = timestamp,
            Success = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            CorrelationId = correlationId
        };
}
