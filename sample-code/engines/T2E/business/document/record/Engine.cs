using DomainRecord = Whycespace.Domain.BusinessSystem.Document.Record.RecordAggregate;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Document.Record;

public sealed class RecordEngine
{
    private readonly RecordPolicyAdapter _policy = new();

    public async Task<EngineResult> ExecuteAsync(RecordCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);
        return command switch
        {
            CreateRecordCommand c => await CreateAsync(c, context, ct),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> CreateAsync(CreateRecordCommand command, EngineContext context, CancellationToken ct)
    {
        var aggregate = await context.LoadAggregate<DomainRecord>(command.Id);
        aggregate.Create(Guid.Parse(command.Id));
        await context.EmitEvents(aggregate);
        return EngineResult.Ok();
    }
}
