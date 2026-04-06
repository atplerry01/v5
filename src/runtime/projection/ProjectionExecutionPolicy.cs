namespace Whyce.Runtime.Projection;

/// <summary>
/// Projection execution policy. Controls how projections are dispatched
/// by the Event Fabric.
///
/// - INLINE: Projection handler runs synchronously within the fabric pipeline.
///   Ensures read model is updated before control returns to caller.
///
/// - ASYNC: Projection handler runs asynchronously via ProjectionAsyncWorker.
///   Decouples projection updates from the main execution path.
/// </summary>
public enum ProjectionExecutionPolicy
{
    Inline,
    Async
}
