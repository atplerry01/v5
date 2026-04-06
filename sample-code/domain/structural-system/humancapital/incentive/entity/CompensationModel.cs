namespace Whycespace.Domain.StructuralSystem.HumanCapital.Incentive;

public sealed class CompensationModel
{
    public Guid Id { get; }
    public string ModelName { get; }

    public CompensationModel(Guid id, string modelName)
    {
        Id = id;
        ModelName = modelName;
    }
}
