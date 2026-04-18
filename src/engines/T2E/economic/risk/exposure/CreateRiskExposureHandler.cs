using Whycespace.Domain.EconomicSystem.Risk.Exposure;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Risk.Exposure;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Observability;

namespace Whycespace.Engines.T2E.Economic.Risk.Exposure;

public sealed class CreateRiskExposureHandler : IEngine
{
    private readonly IEconomicMetrics _metrics;

    public CreateRiskExposureHandler(IEconomicMetrics metrics)
    {
        _metrics = metrics;
    }

    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateRiskExposureCommand cmd)
            return Task.CompletedTask;

        var aggregate = ExposureAggregate.Create(
            new ExposureId(cmd.ExposureId),
            new SourceId(cmd.SourceId),
            (ExposureType)cmd.ExposureType,
            new Amount(cmd.InitialExposure),
            new Currency(cmd.Currency),
            new Timestamp(cmd.CreatedAt));

        context.EmitEvents(aggregate.DomainEvents);
        _metrics.RecordRiskExposureOpened(((ExposureType)cmd.ExposureType).ToString(), cmd.Currency);
        return Task.CompletedTask;
    }
}
