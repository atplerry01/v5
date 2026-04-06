using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Whycespace.Shared.Contracts.Messaging;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.Workers;

/// <summary>
/// Consumes commands via IMessageConsumer and routes through IRuntimeControlPlane.
/// Canonical flow: Runtime → WorkflowOrchestrator → T1M (WSS) → T2E.
/// NO direct T2E engine calls — all execution through the control plane pipeline.
/// </summary>
public class CommandProcessorWorker : BackgroundService
{
    private readonly ILogger<CommandProcessorWorker> _logger;
    private readonly IMessageConsumerFactory _consumerFactory;
    private readonly IRuntimeControlPlane _controlPlane;
    private readonly IClock _clock;

    public CommandProcessorWorker(
        ILogger<CommandProcessorWorker> logger,
        IMessageConsumerFactory consumerFactory,
        IRuntimeControlPlane controlPlane,
        IClock clock)
    {
        _logger = logger;
        _consumerFactory = consumerFactory;
        _controlPlane = controlPlane;
        _clock = clock;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var consumer = _consumerFactory.Create("command-processor");
        consumer.Subscribe("whyce.operational.sandbox.todo.commands");

        _logger.LogInformation("CommandProcessorWorker started — subscribed to command topics");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var message = await consumer.ConsumeAsync(TimeSpan.FromSeconds(1), stoppingToken);
                if (message is null) continue;

                await ProcessCommandAsync(message, stoppingToken);
                consumer.Commit();
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CommandProcessorWorker processing error");
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }

        _logger.LogInformation("CommandProcessorWorker stopped");
    }

    private async Task ProcessCommandAsync(ConsumedMessage message, CancellationToken cancellationToken)
    {
        var envelope = JsonDocument.Parse(message.Value).RootElement;

        var commandType = envelope.GetProperty("command_type").GetString()!;
        var correlationId = envelope.TryGetProperty("correlation_id", out var cid)
            ? cid.GetString()!
            : DeterministicIdHelper.FromSeed($"cmd:{commandType}:{message.Offset}").ToString();
        var payload = envelope.GetProperty("payload");

        _logger.LogInformation(
            "Processing command {CommandType} correlation={CorrelationId}",
            commandType, correlationId);

        var commandId = DeterministicIdHelper.FromSeed($"cmd:{correlationId}:{commandType}");

        var result = await _controlPlane.ExecuteAsync(new RuntimeCommandEnvelope
        {
            CommandId = commandId,
            CommandType = commandType,
            Payload = payload,
            CorrelationId = correlationId,
            Timestamp = _clock.UtcNowOffset
        }, cancellationToken);

        if (result.Success)
        {
            _logger.LogInformation(
                "Command {CommandType} succeeded — correlation={CorrelationId}",
                commandType, correlationId);
        }
        else
        {
            _logger.LogWarning(
                "Command {CommandType} failed — {ErrorCode}: {ErrorMessage} correlation={CorrelationId}",
                commandType, result.ErrorCode, result.ErrorMessage, correlationId);
        }
    }
}
