using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.TechnicalProcessing.Processing;

public sealed record MediaProcessingCompletedEvent(
    [property: JsonPropertyName("AggregateId")] MediaProcessingJobId JobId,
    MediaProcessingOutputRef OutputRef,
    Timestamp CompletedAt) : DomainEvent;
