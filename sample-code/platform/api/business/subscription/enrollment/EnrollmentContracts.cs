namespace Whycespace.Platform.Api.Business.Subscription.Enrollment;

public sealed record EnrollmentRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record EnrollmentResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
