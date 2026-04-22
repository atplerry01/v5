using Whycespace.Domain.TrustSystem.Identity.Credential;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Observability;
using Whycespace.Shared.Contracts.Trust.Identity.Credential;

namespace Whycespace.Engines.T2E.Trust.Identity.Credential;

public sealed class IssueCredentialHandler : IEngine
{
    private readonly ITrustMetrics _metrics;

    public IssueCredentialHandler(ITrustMetrics metrics) => _metrics = metrics;

    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not IssueCredentialCommand cmd)
            return Task.CompletedTask;

        var hashValue = cmd.CredentialHash is not null
            ? new CredentialHashValue(cmd.CredentialHash)
            : (CredentialHashValue?)null;

        var aggregate = CredentialAggregate.Issue(
            new CredentialId(cmd.CredentialId),
            new CredentialDescriptor(cmd.IdentityReference, cmd.CredentialType, hashValue));

        context.EmitEvents(aggregate.DomainEvents);
        _metrics.RecordCredentialIssued(cmd.CredentialType);
        return Task.CompletedTask;
    }
}
