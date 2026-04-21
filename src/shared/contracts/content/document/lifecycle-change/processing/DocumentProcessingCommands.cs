using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Document.LifecycleChange.Processing;

public sealed record RequestDocumentProcessingCommand(
    Guid JobId,
    string Kind,
    Guid InputRef,
    DateTimeOffset RequestedAt) : IHasAggregateId
{
    public Guid AggregateId => JobId;
}

public sealed record StartDocumentProcessingCommand(
    Guid JobId,
    DateTimeOffset StartedAt) : IHasAggregateId
{
    public Guid AggregateId => JobId;
}

public sealed record CompleteDocumentProcessingCommand(
    Guid JobId,
    Guid OutputRef,
    DateTimeOffset CompletedAt) : IHasAggregateId
{
    public Guid AggregateId => JobId;
}

public sealed record FailDocumentProcessingCommand(
    Guid JobId,
    string Reason,
    DateTimeOffset FailedAt) : IHasAggregateId
{
    public Guid AggregateId => JobId;
}

public sealed record CancelDocumentProcessingCommand(
    Guid JobId,
    DateTimeOffset CancelledAt) : IHasAggregateId
{
    public Guid AggregateId => JobId;
}
