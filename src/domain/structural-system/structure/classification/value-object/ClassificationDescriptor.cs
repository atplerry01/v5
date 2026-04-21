using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Structure.Classification;

public readonly record struct ClassificationDescriptor
{
    public string ClassificationName { get; }
    public string ClassificationCategory { get; }

    public ClassificationDescriptor(string classificationName, string classificationCategory)
    {
        Guard.Against(string.IsNullOrWhiteSpace(classificationName), "ClassificationDescriptor name must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(classificationCategory), "ClassificationDescriptor category must not be empty.");

        ClassificationName = classificationName;
        ClassificationCategory = classificationCategory;
    }
}
