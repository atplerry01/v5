namespace Whycespace.Shared.Contracts.Events.Structural.Structure.Classification;

public sealed record ClassificationDefinedEventSchema(
    Guid AggregateId,
    string ClassificationName,
    string ClassificationCategory);

public sealed record ClassificationActivatedEventSchema(
    Guid AggregateId);

public sealed record ClassificationDeprecatedEventSchema(
    Guid AggregateId);
