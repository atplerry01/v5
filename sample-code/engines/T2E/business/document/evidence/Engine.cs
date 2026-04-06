using DomainEvidence = Whycespace.Domain.BusinessSystem.Document.Evidence.EvidenceAggregate;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Document.Evidence;

public sealed class EvidenceEngine
{
    private readonly EvidencePolicyAdapter _policy = new();

    public async Task<EngineResult> ExecuteAsync(EvidenceCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);
        return command switch
        {
            CreateEvidenceCommand c => await CreateAsync(c, context, ct),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> CreateAsync(CreateEvidenceCommand command, EngineContext context, CancellationToken ct)
    {
        var aggregate = await context.LoadAggregate<DomainEvidence>(command.Id);
        aggregate.Create(Guid.Parse(command.Id));
        await context.EmitEvents(aggregate);
        return EngineResult.Ok();
    }
}
