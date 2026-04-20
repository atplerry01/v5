using System.Diagnostics;
using Whycespace.Platform.Api.Middleware;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;

namespace Whycespace.Tests.Integration.Chaos;

/// <summary>
/// R5.C.2 Phase 2 / <c>concurrency-conflict-loop</c> — in-memory
/// end-to-end proof: a <see cref="ConcurrencyConflictException"/> raised
/// inside the event-fabric span produces the full canonical chain:
/// 409 + <c>urn:whyce:error:concurrency-conflict</c> + Activity Error +
/// failure-reason tag.
/// </summary>
public sealed class ConcurrencyConflictChaosLoopTest
{
    [Fact]
    public async Task Full_loop_fires_canonical_chain_for_concurrency_conflict()
    {
        await using var harness = new ChaosLoopHarness();
        var aggregateId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        var proof = await harness.RunAsync(
            wrappingSpanName: "event.fabric.process",
            faultAction: () => throw new ConcurrencyConflictException(
                aggregateId, expectedVersion: 5, actualVersion: 7),
            handler: new ConcurrencyConflictExceptionHandler());

        // Link 1 — canonical exception
        Assert.Equal(nameof(ConcurrencyConflictException), proof.ExceptionType);

        // Link 2 — handler accepted
        Assert.True(proof.HandlerHandled);

        // Link 3 — canonical HTTP status + type URI (the H8b contract)
        Assert.Equal(409, proof.HttpStatus);
        Assert.Equal("application/problem+json", proof.ContentType);
        Assert.Equal("urn:whyce:error:concurrency-conflict", proof.ProblemType());

        // Link 4 — wrapping fabric span recorded Error + exception-type failure-reason
        Assert.NotNull(proof.WrappingActivity);
        Assert.Equal(ActivityStatusCode.Error, proof.WrappingActivity!.Status);
        Assert.Equal(nameof(ConcurrencyConflictException), proof.WrappingActivity.StatusDescription);
        var failureReason = proof.WrappingActivity.GetTagItem("whyce.failure_reason") as string;
        Assert.Equal(nameof(ConcurrencyConflictException), failureReason);
    }
}
