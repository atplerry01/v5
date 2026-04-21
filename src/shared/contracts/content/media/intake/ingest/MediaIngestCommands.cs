using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Media.Intake.Ingest;

public sealed record RequestMediaIngestCommand(
    Guid UploadId,
    Guid SourceRef,
    Guid InputRef,
    DateTimeOffset RequestedAt) : IHasAggregateId
{
    public Guid AggregateId => UploadId;
}

public sealed record AcceptMediaIngestCommand(
    Guid UploadId,
    DateTimeOffset AcceptedAt) : IHasAggregateId
{
    public Guid AggregateId => UploadId;
}

public sealed record StartMediaIngestProcessingCommand(
    Guid UploadId,
    DateTimeOffset StartedAt) : IHasAggregateId
{
    public Guid AggregateId => UploadId;
}

public sealed record CompleteMediaIngestCommand(
    Guid UploadId,
    Guid OutputRef,
    DateTimeOffset CompletedAt) : IHasAggregateId
{
    public Guid AggregateId => UploadId;
}

public sealed record FailMediaIngestCommand(
    Guid UploadId,
    string Reason,
    DateTimeOffset FailedAt) : IHasAggregateId
{
    public Guid AggregateId => UploadId;
}

public sealed record CancelMediaIngestCommand(
    Guid UploadId,
    DateTimeOffset CancelledAt) : IHasAggregateId
{
    public Guid AggregateId => UploadId;
}
