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

    public static DomainException MissingStructuralOwner()
        => new("Document must bind to a structural owner (cluster). Content cannot exist without a structural parent.");

    public static DomainException CannotModifySupersededDocument()
        => new("Cannot modify a superseded document.");

    public static DomainException DocumentAlreadySuperseded()
        => new("Document is already superseded.");

    public static DomainException CannotSupersedeWithSelf()
        => new("Document cannot supersede itself.");

    public static DomainException CannotSupersedeArchivedDocument()
        => new("Cannot supersede an archived document.");
}
