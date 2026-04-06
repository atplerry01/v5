namespace Whycespace.Shared.Contracts.Platform;

/// <summary>
/// Generic query envelope for Platform API read operations.
/// Wraps any typed query with routing metadata and projection hints.
/// No domain entities exposed — primitives only.
/// </summary>
public sealed record QueryRequestDTO
{
    public required string QueryId { get; init; }
    public required string QueryType { get; init; }
    public required int Version { get; init; }
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>Serialized query parameters — deserialized by the handler that owns the QueryType.</summary>
    public required string Payload { get; init; }

    public required Guid IssuedByIdentityId { get; init; }
    public string? CorrelationId { get; init; }
    public int? PageIndex { get; init; }
    public int? PageSize { get; init; }
    public Dictionary<string, string>? Headers { get; init; }
}
