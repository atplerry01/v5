using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Structural.Structure.Classification;

public sealed record DefineClassificationCommand(
    Guid ClassificationId,
    string ClassificationName,
    string ClassificationCategory) : IHasAggregateId
{
    public Guid AggregateId => ClassificationId;
}

public sealed record ActivateClassificationCommand(
    Guid ClassificationId) : IHasAggregateId
{
    public Guid AggregateId => ClassificationId;
}

public sealed record DeprecateClassificationCommand(
    Guid ClassificationId) : IHasAggregateId
{
    public Guid AggregateId => ClassificationId;
}
