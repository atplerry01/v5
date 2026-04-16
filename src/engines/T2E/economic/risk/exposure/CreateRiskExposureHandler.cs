using Whycespace.Domain.EconomicSystem.Risk.Exposure;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Risk.Exposure;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Risk.Exposure;

public sealed class CreateRiskExposureHandler : IEngine
{
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
        return Task.CompletedTask;
    }
}
