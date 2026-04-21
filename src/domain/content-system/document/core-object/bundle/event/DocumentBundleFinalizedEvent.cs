using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Bundle;

public sealed record DocumentBundleFinalizedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentBundleId BundleId,
    Timestamp FinalizedAt) : DomainEvent;
