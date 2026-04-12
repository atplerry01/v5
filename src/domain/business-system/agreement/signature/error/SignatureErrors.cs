namespace Whycespace.Domain.BusinessSystem.Agreement.Signature;

public static class SignatureErrors
{
    public static SignatureDomainException MissingId()
        => new("SignatureId is required and must not be empty.");

    public static SignatureDomainException AlreadySigned(SignatureId id)
        => new($"Signature '{id.Value}' has already been signed.");

    public static SignatureDomainException AlreadyRevoked(SignatureId id)
        => new($"Signature '{id.Value}' has already been revoked.");

    public static SignatureDomainException InvalidStateTransition(SignatureStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class SignatureDomainException : Exception
{
    public SignatureDomainException(string message) : base(message) { }
}
