namespace Whycespace.Platform.Api.Trust.Identity.Identity;

public sealed record RegisterIdentityDto { public string? IdentityId { get; init; } public required string IdentityType { get; init; } public required string DisplayName { get; init; } }
public sealed record ActivateIdentityDto { public required string IdentityId { get; init; } }
public sealed record SuspendIdentityDto { public required string IdentityId { get; init; } public required string Reason { get; init; } }
public sealed record DeactivateIdentityDto { public required string IdentityId { get; init; } public required string Reason { get; init; } }
public sealed record AuthenticateDto { public required string IdentityId { get; init; } public required string CredentialType { get; init; } public required string CredentialValue { get; init; } public string? DeviceId { get; init; } }
public sealed record IdentityQueryResponse { public required string IdentityId { get; init; } public required string IdentityType { get; init; } public required string DisplayName { get; init; } public required string Status { get; init; } }
