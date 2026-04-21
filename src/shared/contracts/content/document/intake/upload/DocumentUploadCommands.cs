using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Document.Intake.Upload;

public sealed record RequestDocumentUploadCommand(
    Guid UploadId,
    Guid SourceRef,
    Guid InputRef,
    DateTimeOffset RequestedAt) : IHasAggregateId
{
    public Guid AggregateId => UploadId;
}

public sealed record AcceptDocumentUploadCommand(
    Guid UploadId,
    DateTimeOffset AcceptedAt) : IHasAggregateId
{
    public Guid AggregateId => UploadId;
}

public sealed record StartDocumentUploadProcessingCommand(
    Guid UploadId,
    DateTimeOffset StartedAt) : IHasAggregateId
{
    public Guid AggregateId => UploadId;
}

public sealed record CompleteDocumentUploadCommand(
    Guid UploadId,
    Guid OutputRef,
    DateTimeOffset CompletedAt) : IHasAggregateId
{
    public Guid AggregateId => UploadId;
}

public sealed record FailDocumentUploadCommand(
    Guid UploadId,
    string Reason,
    DateTimeOffset FailedAt) : IHasAggregateId
{
    public Guid AggregateId => UploadId;
}

public sealed record CancelDocumentUploadCommand(
    Guid UploadId,
    DateTimeOffset CancelledAt) : IHasAggregateId
{
    public Guid AggregateId => UploadId;
}
