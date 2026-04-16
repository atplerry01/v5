using Microsoft.Extensions.DependencyInjection;
using Whycespace.Shared.Contracts.Economic.Compliance.Audit;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Economic.Compliance;

public static class CompliancePolicyModule
{
    public static IServiceCollection AddCompliancePolicyBindings(this IServiceCollection services)
    {
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateAuditRecordCommand),   AuditRecordPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(FinalizeAuditRecordCommand), AuditRecordPolicyIds.Finalize));
        services.AddSingleton(new CommandPolicyBinding(typeof(GetAuditRecordByIdQuery),    AuditRecordPolicyIds.Read));
        return services;
    }
}
