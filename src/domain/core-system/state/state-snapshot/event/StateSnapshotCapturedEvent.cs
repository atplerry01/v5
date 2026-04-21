using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.State.StateSnapshot;

public sealed record StateSnapshotCapturedEvent(
    [property: JsonPropertyName("AggregateId")] StateSnapshotId SnapshotId,
    SnapshotDescriptor Descriptor) : DomainEvent;
