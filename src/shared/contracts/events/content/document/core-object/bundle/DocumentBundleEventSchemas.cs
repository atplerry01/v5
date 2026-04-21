namespace Whycespace.Shared.Contracts.Events.Content.Document.CoreObject.Bundle;

public sealed record DocumentBundleCreatedEventSchema(
    Guid AggregateId,
    string Name,
    DateTimeOffset CreatedAt);

public sealed record DocumentBundleRenamedEventSchema(
    Guid AggregateId,
    string PreviousName,
    string NewName,
    DateTimeOffset RenamedAt);

public sealed record DocumentBundleMemberAddedEventSchema(
    Guid AggregateId,
    Guid MemberId,
    DateTimeOffset AddedAt);

public sealed record DocumentBundleMemberRemovedEventSchema(
    Guid AggregateId,
    Guid MemberId,
    DateTimeOffset RemovedAt);

public sealed record DocumentBundleFinalizedEventSchema(
    Guid AggregateId,
    DateTimeOffset FinalizedAt);

public sealed record DocumentBundleArchivedEventSchema(
    Guid AggregateId,
    DateTimeOffset ArchivedAt);
