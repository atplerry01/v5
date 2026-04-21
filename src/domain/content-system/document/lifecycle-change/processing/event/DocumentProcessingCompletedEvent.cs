using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.LifecycleChange.Processing;

public sealed record DocumentProcessingCompletedEvent(
    [property: JsonPropertyName("AggregateId")] ProcessingJobId JobId,
    ProcessingOutputRef OutputRef,
    Timestamp CompletedAt) : DomainEvent;
