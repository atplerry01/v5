using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.LifecycleChange.Processing;

public sealed record DocumentProcessingStartedEvent(
    [property: JsonPropertyName("AggregateId")] ProcessingJobId JobId,
    Timestamp StartedAt) : DomainEvent;
