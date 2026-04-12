namespace Whycespace.Domain.TrustSystem.Access.Authorization;

public static class AuthorizationErrors
{
    public static InvalidOperationException MissingId() =>
        new("AuthorizationId is required and must not be empty.");

    public static InvalidOperationException MissingScope() =>
        new("AuthorizationScope is required and must not be empty.");

    public static InvalidOperationException InvalidStateTransition(AuthorizationStatus status, string action) =>
        new($"Cannot '{action}' when current status is '{status}'.");
}
