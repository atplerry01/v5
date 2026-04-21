using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.LifecycleChange.Processing;

public sealed record DocumentProcessingFailedEvent(
    [property: JsonPropertyName("AggregateId")] ProcessingJobId JobId,
    ProcessingFailureReason Reason,
    Timestamp FailedAt) : DomainEvent;
