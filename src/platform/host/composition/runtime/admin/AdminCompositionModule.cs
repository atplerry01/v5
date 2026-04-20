using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Adapters.Admin;
using Whycespace.Runtime.ControlPlane.Admin;
using Whycespace.Shared.Contracts.Runtime.Admin;

namespace Whycespace.Platform.Host.Composition.Runtime.Admin;

/// <summary>
/// R4.B — composition for the admin/operator control surface. Registers:
///
/// <list type="bullet">
///   <item><see cref="IOperatorActionRecorder"/> — routes operator-action
///   events onto the runtime audit stream via the event fabric.</item>
///   <item><see cref="IDeadLetterRedriveService"/> + its
///   <see cref="IDeadLetterRedrivePublisher"/> — Kafka-backed DLQ re-drive
///   with eligibility gate + audit emission.</item>
///   <item><see cref="IRequestCorrelationAccessor"/> — reads the correlation
///   id from the ambient HTTP request for operator-action audit linking.</item>
/// </list>
///
/// Registered as a standalone module so the admin surface can be toggled or
/// swapped without entangling it with the rest of the runtime composition.
/// Authentication + authorization are registered separately in
/// <c>InfrastructureCompositionRoot</c> (AddAuthentication + AddAdminAuthorization).
/// </summary>
public static class AdminCompositionModule
{
    public static IServiceCollection AddAdminCompositionModule(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<IRequestCorrelationAccessor, HttpRequestCorrelationAccessor>();
        services.AddSingleton<IOperatorActionRecorder, OperatorActionAuditRecorder>();
        services.AddSingleton<IDeadLetterRedrivePublisher, KafkaDeadLetterRedrivePublisher>();
        services.AddSingleton<IDeadLetterRedriveService, DeadLetterRedriveService>();
        return services;
    }
}
