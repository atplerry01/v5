using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.SystemPolicy.PolicyEvaluation;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyEvaluation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.SystemPolicy.PolicyEvaluation.Application;

public static class PolicyEvaluationApplicationModule
{
    public static IServiceCollection AddPolicyEvaluationApplication(this IServiceCollection services)
    {
        services.AddTransient<RecordPolicyEvaluationHandler>();
        services.AddTransient<IssuePolicyEvaluationVerdictHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RecordPolicyEvaluationCommand, RecordPolicyEvaluationHandler>();
        engine.Register<IssuePolicyEvaluationVerdictCommand, IssuePolicyEvaluationVerdictHandler>();
    }
}
