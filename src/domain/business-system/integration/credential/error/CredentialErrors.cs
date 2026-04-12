namespace Whycespace.Domain.BusinessSystem.Integration.Credential;

public static class CredentialErrors
{
    public static InvalidOperationException MissingId() =>
        new("CredentialId is required and must not be empty.");

    public static InvalidOperationException MissingDescriptor() =>
        new("Credential must include a valid descriptor.");

    public static InvalidOperationException InvalidStateTransition(CredentialStatus current, string attemptedAction) =>
        new($"Cannot '{attemptedAction}' when current status is '{current}'.");
}
