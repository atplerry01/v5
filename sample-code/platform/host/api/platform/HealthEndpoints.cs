using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Platform.Api.Platform;

public static class HealthEndpoints
{
    public static WebApplication MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/health", (IClock clock) =>
            Results.Ok(new { status = "healthy", timestamp = clock.UtcNow }))
            .WithTags("Health");

        return app;
    }
}
