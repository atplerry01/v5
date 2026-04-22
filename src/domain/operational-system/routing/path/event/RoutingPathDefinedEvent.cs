using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OperationalSystem.Routing.Path;

// β #2 / D10: schema-mapped JSONB stores the identity under the generic key
// "AggregateId"; the [property: JsonPropertyName] mapping bridges to the
// typed PathId parameter so STJ's positional-record binding finds the value
// on replay. Without this the constructor parameter is missing and PathId
// reconstructs as default(PathId) = Guid.Empty.
public sealed record RoutingPathDefinedEvent(
    [property: JsonPropertyName("AggregateId")] PathId PathId,
    PathType PathType,
    string Conditions,
    int Priority) : DomainEvent;
