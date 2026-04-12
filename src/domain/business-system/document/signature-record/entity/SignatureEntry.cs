namespace Whycespace.Domain.BusinessSystem.Document.SignatureRecord;

public sealed class SignatureEntry
{
    public Guid EntryId { get; }
    public Guid SignerId { get; }
    public Guid SourceDocumentId { get; }
    public string SignatureHash { get; }

    public SignatureEntry(Guid entryId, Guid signerId, Guid sourceDocumentId, string signatureHash)
    {
        if (entryId == Guid.Empty)
            throw new ArgumentException("EntryId must not be empty.", nameof(entryId));

        if (signerId == Guid.Empty)
            throw new ArgumentException("SignerId must not be empty.", nameof(signerId));

        if (sourceDocumentId == Guid.Empty)
            throw new ArgumentException("SourceDocumentId must not be empty.", nameof(sourceDocumentId));

        if (string.IsNullOrWhiteSpace(signatureHash))
            throw new ArgumentException("SignatureHash must not be empty.", nameof(signatureHash));

        EntryId = entryId;
        SignerId = signerId;
        SourceDocumentId = sourceDocumentId;
        SignatureHash = signatureHash;
    }
}
