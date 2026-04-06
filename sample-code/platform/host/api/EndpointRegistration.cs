using Whycespace.Platform.Api.Governance.Policy;
using Whycespace.Platform.Api.Operational.Incident;
using Whycespace.Platform.Api.Operational.Sandbox.Todo;
using Whycespace.Platform.Api.Platform;

namespace Whycespace.Platform.Api;

public static class EndpointRegistration
{
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        // Platform
        app.MapHealthEndpoints();
        app.MapPingEndpoints();

        // Operational — incident (global context)
        app.MapIncidentEndpoints();

        // Operational — sandbox
        app.MapTodoEndpoints();

        // Governance — policy management
        app.MapPolicyEndpoints();

        // Governance — policy simulation (T3I intelligence)
        app.MapPolicySimulationEndpoints();

        // Governance — policy federation (cross-cluster policy graph)
        app.MapPolicyFederationEndpoints();

        return app;
    }
}
