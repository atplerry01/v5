using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceWindow;

public sealed record ServiceWindowArchivedEvent(
    [property: JsonPropertyName("AggregateId")] ServiceWindowId ServiceWindowId) : DomainEvent;
