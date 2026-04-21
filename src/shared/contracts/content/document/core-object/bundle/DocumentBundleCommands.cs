using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Document.CoreObject.Bundle;

public sealed record CreateDocumentBundleCommand(
    Guid BundleId,
    string Name,
    DateTimeOffset CreatedAt) : IHasAggregateId
{
    public Guid AggregateId => BundleId;
}

public sealed record RenameDocumentBundleCommand(
    Guid BundleId,
    string NewName,
    DateTimeOffset RenamedAt) : IHasAggregateId
{
    public Guid AggregateId => BundleId;
}

public sealed record AddDocumentBundleMemberCommand(
    Guid BundleId,
    Guid MemberId,
    DateTimeOffset AddedAt) : IHasAggregateId
{
    public Guid AggregateId => BundleId;
}

public sealed record RemoveDocumentBundleMemberCommand(
    Guid BundleId,
    Guid MemberId,
    DateTimeOffset RemovedAt) : IHasAggregateId
{
    public Guid AggregateId => BundleId;
}

public sealed record FinalizeDocumentBundleCommand(
    Guid BundleId,
    DateTimeOffset FinalizedAt) : IHasAggregateId
{
    public Guid AggregateId => BundleId;
}

public sealed record ArchiveDocumentBundleCommand(
    Guid BundleId,
    DateTimeOffset ArchivedAt) : IHasAggregateId
{
    public Guid AggregateId => BundleId;
}
