using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Media.LifecycleChange.Version;

public sealed record CreateMediaVersionCommand(
    Guid VersionId,
    Guid AssetRef,
    int VersionMajor,
    int VersionMinor,
    Guid FileRef,
    Guid? PreviousVersionId,
    DateTimeOffset CreatedAt) : IHasAggregateId
{
    public Guid AggregateId => VersionId;
}

public sealed record ActivateMediaVersionCommand(
    Guid VersionId,
    DateTimeOffset ActivatedAt) : IHasAggregateId
{
    public Guid AggregateId => VersionId;
}

public sealed record SupersedeMediaVersionCommand(
    Guid VersionId,
    Guid SuccessorVersionId,
    DateTimeOffset SupersededAt) : IHasAggregateId
{
    public Guid AggregateId => VersionId;
}

public sealed record WithdrawMediaVersionCommand(
    Guid VersionId,
    string Reason,
    DateTimeOffset WithdrawnAt) : IHasAggregateId
{
    public Guid AggregateId => VersionId;
}
