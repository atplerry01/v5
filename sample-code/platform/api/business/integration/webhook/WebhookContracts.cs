namespace Whycespace.Platform.Api.Business.Integration.Webhook;

public sealed record WebhookRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record WebhookResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
