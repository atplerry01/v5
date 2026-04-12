namespace Whycespace.Domain.BusinessSystem.Document.ContractDocument;

public sealed class DocumentSection
{
    public Guid SectionId { get; }
    public string Title { get; }

    public DocumentSection(Guid sectionId, string title)
    {
        if (sectionId == Guid.Empty)
            throw new ArgumentException("SectionId must not be empty.", nameof(sectionId));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title must not be empty.", nameof(title));

        SectionId = sectionId;
        Title = title;
    }
}
