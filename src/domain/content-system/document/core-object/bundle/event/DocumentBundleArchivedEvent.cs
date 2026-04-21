using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Bundle;

public sealed record DocumentBundleArchivedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentBundleId BundleId,
    Timestamp ArchivedAt) : DomainEvent;
