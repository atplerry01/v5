using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.LifecycleChange.Processing;

public sealed record DocumentProcessingRequestedEvent(
    [property: JsonPropertyName("AggregateId")] ProcessingJobId JobId,
    ProcessingKind Kind,
    ProcessingInputRef InputRef,
    Timestamp RequestedAt) : DomainEvent;
