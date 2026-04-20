using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.File;

public static class DocumentFileErrors
{
    public static DomainException ChecksumMismatch(string expected, string actual)
        => new($"Checksum mismatch: expected '{expected}', actual '{actual}'.");

    public static DomainException FileAlreadyVerified()
        => new("Document file integrity has already been verified.");

    public static DomainException FileAlreadySuperseded()
        => new("Document file is already superseded.");

    public static DomainException FileArchived()
        => new("Cannot mutate an archived document file.");

    public static DomainException AlreadyArchived()
        => new("Document file is already archived.");

    public static DomainException CannotSupersedeWithSelf()
        => new("Successor file id must differ from current file id.");

    public static DomainInvariantViolationException OrphanedDocumentFile()
        => new("Document file must reference an owning document.");
}
