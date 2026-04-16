using Microsoft.Extensions.DependencyInjection;
using Whycespace.Shared.Contracts.Economic.Subject.Subject;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Economic.Subject;

/// <summary>
/// E5 — subject context policy bindings. Registers one
/// <see cref="CommandPolicyBinding"/> per subject command, mapping the command
/// CLR type to its canonical policy id constant declared on
/// <see cref="SubjectPolicyIds"/>. Bindings are aggregated by
/// <c>ICommandPolicyIdRegistry</c>; once registered, every dispatch of a
/// subject command stamps the correct policy id onto
/// <c>CommandContext.PolicyId</c> for evaluation by <c>PolicyMiddleware</c>.
/// </summary>
public static class SubjectPolicyModule
{
    public static IServiceCollection AddSubjectPolicyBindings(this IServiceCollection services)
    {
        services.AddSingleton(new CommandPolicyBinding(
            typeof(RegisterEconomicSubjectCommand),
            SubjectPolicyIds.Register));

        return services;
    }
}
