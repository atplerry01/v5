namespace Whycespace.Platform.Api.Core.Contracts.Access;

/// <summary>
/// Request for an access decision preview.
/// Platform forwards this to the runtime adapter — does NOT compute locally.
/// </summary>
public sealed record AccessPreviewRequest
{
    public required string Resource { get; init; }
    public required string Action { get; init; }
}
