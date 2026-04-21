using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.TechnicalProcessing.Processing;

public sealed record MediaProcessingStartedEvent(
    [property: JsonPropertyName("AggregateId")] MediaProcessingJobId JobId,
    Timestamp StartedAt) : DomainEvent;
