using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Bundle;

public sealed record DocumentBundleCreatedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentBundleId BundleId,
    BundleName Name,
    Timestamp CreatedAt) : DomainEvent;
