using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.State.StateSnapshot;

public sealed record StateSnapshotExpiredEvent(
    [property: JsonPropertyName("AggregateId")] StateSnapshotId SnapshotId) : DomainEvent;
