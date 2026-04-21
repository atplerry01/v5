using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Document.LifecycleChange.Version;

public sealed record CreateDocumentVersionCommand(
    Guid VersionId,
    Guid DocumentRef,
    int Major,
    int Minor,
    Guid ArtifactRef,
    Guid? PreviousVersionId,
    DateTimeOffset CreatedAt) : IHasAggregateId
{
    public Guid AggregateId => VersionId;
}

public sealed record ActivateDocumentVersionCommand(
    Guid VersionId,
    DateTimeOffset ActivatedAt) : IHasAggregateId
{
    public Guid AggregateId => VersionId;
}

public sealed record SupersedeDocumentVersionCommand(
    Guid VersionId,
    Guid SuccessorVersionId,
    DateTimeOffset SupersededAt) : IHasAggregateId
{
    public Guid AggregateId => VersionId;
}

public sealed record WithdrawDocumentVersionCommand(
    Guid VersionId,
    string Reason,
    DateTimeOffset WithdrawnAt) : IHasAggregateId
{
    public Guid AggregateId => VersionId;
}
