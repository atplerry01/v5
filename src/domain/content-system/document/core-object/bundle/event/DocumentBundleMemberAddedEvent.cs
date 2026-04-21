using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Bundle;

public sealed record DocumentBundleMemberAddedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentBundleId BundleId,
    BundleMemberRef Member,
    Timestamp AddedAt) : DomainEvent;
