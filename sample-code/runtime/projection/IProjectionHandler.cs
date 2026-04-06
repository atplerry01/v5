using System.Text.Json;

namespace Whycespace.Runtime.Projection;

public interface IProjectionHandler
{
    string EventType { get; }
    Task HandleAsync(JsonElement eventData, JsonElement metadata, CancellationToken cancellationToken);
}
