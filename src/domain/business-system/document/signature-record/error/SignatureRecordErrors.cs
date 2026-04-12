namespace Whycespace.Domain.BusinessSystem.Document.SignatureRecord;

public static class SignatureRecordErrors
{
    public static SignatureRecordDomainException MissingId()
        => new("SignatureRecordId is required and must not be empty.");

    public static SignatureRecordDomainException InvalidStateTransition(SignatureRecordStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static SignatureRecordDomainException SignatureEntryRequired()
        => new("Signature record must contain at least one signature entry.");

    public static SignatureRecordDomainException ModificationAfterVerification()
        => new("Cannot modify signature record after verification.");
}
