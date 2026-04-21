using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Bundle;

public sealed record DocumentBundleMemberRemovedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentBundleId BundleId,
    BundleMemberRef Member,
    Timestamp RemovedAt) : DomainEvent;
