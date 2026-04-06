namespace Whycespace.Shared.Contracts.Platform;

/// <summary>
/// Generic command envelope for Platform API ingress.
/// Wraps any typed command payload with routing and traceability metadata.
/// No domain entities exposed — primitives only.
/// </summary>
public sealed record CommandRequestDTO
{
    public required string CommandId { get; init; }
    public required string CommandType { get; init; }
    public required int Version { get; init; }
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>Serialized command payload — deserialized by the handler that owns the CommandType.</summary>
    public required string Payload { get; init; }

    public required Guid IssuedByIdentityId { get; init; }
    public string? CorrelationId { get; init; }
    public string? CausationId { get; init; }
    public string? IdempotencyKey { get; init; }
    public string? TargetWorkflowType { get; init; }
    public Dictionary<string, string>? Headers { get; init; }
}
