namespace Whycespace.Domain.BusinessSystem.Document.Template;

public sealed class TemplateStructure
{
    public Guid SectionId { get; }
    public string Name { get; }
    public string Definition { get; }

    public TemplateStructure(Guid sectionId, string name, string definition)
    {
        if (sectionId == Guid.Empty)
            throw new ArgumentException("SectionId must not be empty.", nameof(sectionId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name must not be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(definition))
            throw new ArgumentException("Definition must not be empty.", nameof(definition));

        SectionId = sectionId;
        Name = name;
        Definition = definition;
    }
}
