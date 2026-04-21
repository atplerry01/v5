using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Media.TechnicalProcessing.Processing;

public sealed record RequestMediaProcessingCommand(
    Guid JobId,
    string Kind,
    Guid InputRef,
    DateTimeOffset RequestedAt) : IHasAggregateId
{
    public Guid AggregateId => JobId;
}

public sealed record StartMediaProcessingCommand(
    Guid JobId,
    DateTimeOffset StartedAt) : IHasAggregateId
{
    public Guid AggregateId => JobId;
}

public sealed record CompleteMediaProcessingCommand(
    Guid JobId,
    Guid OutputRef,
    DateTimeOffset CompletedAt) : IHasAggregateId
{
    public Guid AggregateId => JobId;
}

public sealed record FailMediaProcessingCommand(
    Guid JobId,
    string Reason,
    DateTimeOffset FailedAt) : IHasAggregateId
{
    public Guid AggregateId => JobId;
}

public sealed record CancelMediaProcessingCommand(
    Guid JobId,
    DateTimeOffset CancelledAt) : IHasAggregateId
{
    public Guid AggregateId => JobId;
}
