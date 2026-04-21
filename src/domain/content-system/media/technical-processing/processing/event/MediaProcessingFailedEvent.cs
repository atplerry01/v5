using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.TechnicalProcessing.Processing;

public sealed record MediaProcessingFailedEvent(
    [property: JsonPropertyName("AggregateId")] MediaProcessingJobId JobId,
    MediaProcessingFailureReason Reason,
    Timestamp FailedAt) : DomainEvent;
