namespace Whycespace.Domain.BusinessSystem.Integration.Token;

public static class TokenErrors
{
    public static InvalidOperationException MissingId()
        => new("TokenId is required and must not be empty.");

    public static InvalidOperationException MissingDescriptor()
        => new("TokenDescriptor is required and must not be default.");

    public static InvalidOperationException InvalidStateTransition(TokenStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}
