using System.Diagnostics;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Platform.Api.Middleware;

namespace Whycespace.Tests.Integration.Chaos;

/// <summary>
/// R5.C.2 Phase 2 / <c>domain-invariant-violation-loop</c> — in-memory
/// end-to-end proof: throwing a <see cref="DomainException"/> inside a
/// wrapping span runs through <see cref="DomainExceptionHandler"/> and
/// produces the full canonical chain: 400 + canonical type URI + Activity
/// Error with exception-type failure-reason.
/// </summary>
public sealed class DomainInvariantChaosLoopTest
{
    [Fact]
    public async Task Full_loop_fires_canonical_chain_for_domain_invariant_violation()
    {
        await using var harness = new ChaosLoopHarness();

        var proof = await harness.RunAsync(
            wrappingSpanName: "runtime.command.dispatch",
            faultAction: () => throw new DomainException("account balance must be non-negative"),
            handler: new DomainExceptionHandler());

        // Link 1 — canonical exception type
        Assert.Equal(nameof(DomainException), proof.ExceptionType);

        // Link 2 — canonical handler accepted the exception
        Assert.True(proof.HandlerHandled);

        // Link 3 — canonical HTTP status + type URI
        Assert.Equal(400, proof.HttpStatus);
        Assert.Equal("application/problem+json", proof.ContentType);
        Assert.Equal("urn:whyce:error:domain-invariant", proof.ProblemType());

        // Link 4 — wrapping Activity recorded Error + exception-type failure-reason
        Assert.NotNull(proof.WrappingActivity);
        Assert.Equal(ActivityStatusCode.Error, proof.WrappingActivity!.Status);
        Assert.Equal(nameof(DomainException), proof.WrappingActivity.StatusDescription);
        var failureReason = proof.WrappingActivity.GetTagItem("whyce.failure_reason") as string;
        Assert.Equal(nameof(DomainException), failureReason);
    }
}
