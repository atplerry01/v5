using DomainContract = Whycespace.Domain.BusinessSystem.Document.ContractDocument.ContractAggregate;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Document.ContractDocument;

public sealed class ContractDocumentEngine
{
    private readonly ContractDocumentPolicyAdapter _policy = new();

    public async Task<EngineResult> ExecuteAsync(ContractDocumentCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);
        return command switch
        {
            CreateContractDocumentCommand c => await CreateAsync(c, context, ct),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> CreateAsync(CreateContractDocumentCommand command, EngineContext context, CancellationToken ct)
    {
        var aggregate = await context.LoadAggregate<DomainContract>(command.Id);
        aggregate.Create(Guid.Parse(command.Id));
        await context.EmitEvents(aggregate);
        return EngineResult.Ok();
    }
}
