using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Document;

public static class DocumentErrors
{
    public static DomainException DocumentAlreadyArchived()
        => new("Document is already archived.");

    public static DomainException DocumentNotArchived()
        => new("Document is not archived.");

    public static DomainException DocumentAlreadyActive()
        => new("Document is already active.");

    public static DomainException CannotModifyArchivedDocument()
        => new("Cannot modify an archived document.");
}
