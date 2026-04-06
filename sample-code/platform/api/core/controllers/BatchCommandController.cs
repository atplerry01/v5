using Whycespace.Platform.Api.Core.Contracts;
using Whycespace.Platform.Api.Core.Guards;
using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Core.Controllers;

/// <summary>
/// Batch command execution controller.
/// Accepts multiple commands and dispatches them as a batch.
///
/// PLATFORM GUARDS:
/// - No direct engine or runtime calls
/// - All commands flow through DownstreamAdapter
/// - Each command is independently validated and policy-checked
/// - Batch results are collected and returned as a single response
/// - Supports async tracking via batch correlation ID
/// </summary>
public sealed class BatchCommandController
{
    private readonly Func<ApiRequest, Task<ApiResponse>> _singleCommandHandler;

    public BatchCommandController(Func<ApiRequest, Task<ApiResponse>> singleCommandHandler)
    {
        _singleCommandHandler = singleCommandHandler ?? throw new ArgumentNullException(nameof(singleCommandHandler));
    }

    public async Task<ApiResponse> HandleAsync(ApiRequest request, CancellationToken cancellationToken = default)
    {
        var guardViolation = PlatformGuard.EnforceAll(request);
        if (guardViolation is not null)
            return guardViolation;

        var batchRequest = request.Body as BatchCommandRequest;
        if (batchRequest is null || batchRequest.Commands.Count == 0)
            return ApiResponse.BadRequest("Invalid or empty batch command request", request.TraceId);

        if (batchRequest.Commands.Count > 50)
            return ApiResponse.BadRequest("Batch size exceeds maximum of 50 commands", request.TraceId);

        var batchCorrelationId = request.Headers.GetValueOrDefault("X-Correlation-Id")
            ?? request.RequestId;

        var results = new List<BatchCommandResult>();

        foreach (var command in batchRequest.Commands)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var commandRequest = new ApiRequest
            {
                Method = "POST",
                Endpoint = $"/api/v1/{command.Domain}/{command.Action}",
                Body = command.Payload,
                Headers = request.Headers,
                RequestId = $"{batchCorrelationId}:{command.Index}",
                TraceId = request.TraceId,
                WhyceId = request.WhyceId
            };

            var response = await _singleCommandHandler(commandRequest);

            results.Add(new BatchCommandResult(
                command.Index,
                command.Domain,
                command.Action,
                response.StatusCode,
                response.Data));
        }

        var allSucceeded = results.All(r => r.StatusCode >= 200 && r.StatusCode < 300);
        var batchResponse = new BatchCommandResponse(
            batchCorrelationId,
            allSucceeded ? "Completed" : "PartialFailure",
            results);

        return allSucceeded
            ? ApiResponse.Ok(batchResponse, request.TraceId)
            : ApiResponse.Ok(batchResponse, request.TraceId);
    }
}

public sealed record BatchCommandRequest(IReadOnlyList<BatchCommand> Commands);

public sealed record BatchCommand(
    int Index,
    string Domain,
    string Action,
    object Payload);

public sealed record BatchCommandResponse(
    string BatchCorrelationId,
    string Status,
    IReadOnlyList<BatchCommandResult> Results);

public sealed record BatchCommandResult(
    int Index,
    string Domain,
    string Action,
    int StatusCode,
    object? Body);
