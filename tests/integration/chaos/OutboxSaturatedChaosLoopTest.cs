using System.Diagnostics;
using System.Text.Json;
using Whycespace.Platform.Api.Middleware;
using Whycespace.Shared.Contracts.Infrastructure.Messaging;

namespace Whycespace.Tests.Integration.Chaos;

/// <summary>
/// R5.C.2 Phase 2 / <c>outbox-saturated-loop</c> — in-memory end-to-end
/// proof: an <see cref="OutboxSaturatedException"/> raised inside the
/// event-fabric span produces the full canonical chain: 503 +
/// <c>urn:whyce:error:outbox-saturated</c> + <c>Retry-After</c> header +
/// Activity Error + failure-reason tag.
/// </summary>
public sealed class OutboxSaturatedChaosLoopTest
{
    [Fact]
    public async Task Full_loop_fires_canonical_chain_for_outbox_saturation()
    {
        await using var harness = new ChaosLoopHarness();

        var proof = await harness.RunAsync(
            wrappingSpanName: "event.fabric.process",
            faultAction: () => throw new OutboxSaturatedException(
                observedDepth: 15000,
                highWaterMark: 10000,
                retryAfterSeconds: 5),
            handler: new OutboxSaturatedExceptionHandler());

        Assert.Equal(nameof(OutboxSaturatedException), proof.ExceptionType);
        Assert.True(proof.HandlerHandled);

        // Canonical RETRYABLE REFUSAL shape
        Assert.Equal(503, proof.HttpStatus);
        Assert.Equal("application/problem+json", proof.ContentType);
        Assert.Equal("urn:whyce:error:outbox-saturated", proof.ProblemType());

        // Canonical extension fields on the ProblemDetails payload
        using var doc = JsonDocument.Parse(proof.ResponseBody);
        Assert.Equal(15000, doc.RootElement.GetProperty("observedDepth").GetInt64());
        Assert.Equal(10000, doc.RootElement.GetProperty("highWaterMark").GetInt64());
        Assert.Equal(5, doc.RootElement.GetProperty("retryAfterSeconds").GetInt32());

        // Wrapping fabric span error-stamped
        Assert.NotNull(proof.WrappingActivity);
        Assert.Equal(ActivityStatusCode.Error, proof.WrappingActivity!.Status);
        var failureReason = proof.WrappingActivity.GetTagItem("whyce.failure_reason") as string;
        Assert.Equal(nameof(OutboxSaturatedException), failureReason);
    }
}
