namespace Whycespace.Domain.StructuralSystem.HumanCapital.Workforce;

public sealed class EmploymentModel
{
    public Guid Id { get; }
    public string ModelType { get; }

    public EmploymentModel(Guid id, string modelType)
    {
        Id = id;
        ModelType = modelType;
    }
}
