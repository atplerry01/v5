namespace Whycespace.Domain.StructuralSystem.Structure.Classification;

public readonly record struct ClassificationDescriptor
{
    public string ClassificationName { get; }
    public string ClassificationCategory { get; }

    public ClassificationDescriptor(string classificationName, string classificationCategory)
    {
        if (string.IsNullOrWhiteSpace(classificationName))
            throw ClassificationErrors.MissingDescriptor();

        if (string.IsNullOrWhiteSpace(classificationCategory))
            throw ClassificationErrors.MissingDescriptor();

        ClassificationName = classificationName;
        ClassificationCategory = classificationCategory;
    }
}
