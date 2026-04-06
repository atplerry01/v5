namespace Whycespace.Platform.Api.Structural.Humancapital.Participant;

public sealed record ParticipantRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ParticipantResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
