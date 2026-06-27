// <copyright file="TraceContextSnapshot.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
using System.Collections.ObjectModel;
using Atya.Diagnostics.Tracing.Tags;
using Atya.Foundation.Guards;

namespace Atya.Diagnostics.Tracing.Context;

/// <summary>
/// Represents a small immutable snapshot of the current trace context.
/// </summary>
public sealed record TraceContextSnapshot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TraceContextSnapshot"/> class.
    /// </summary>
    /// <param name="traceId">The current trace id.</param>
    /// <param name="spanId">The current span id.</param>
    /// <param name="traceParent">The current traceparent value.</param>
    /// <param name="traceState">The current tracestate value.</param>
    /// <param name="correlationId">The current correlation id when available.</param>
    public TraceContextSnapshot(
        string traceId,
        string spanId,
        string? traceParent,
        string? traceState,
        string? correlationId)
    {
        TraceId = Guard.AgainstNullOrWhiteSpace(traceId);
        SpanId = Guard.AgainstNullOrWhiteSpace(spanId);
        TraceParent = NormalizeOptionalValue(traceParent);
        TraceState = NormalizeOptionalValue(traceState);
        CorrelationId = NormalizeOptionalValue(correlationId);
    }

    /// <summary>
    /// Gets the current trace id.
    /// </summary>
    public string TraceId { get; init; }

    /// <summary>
    /// Gets the current span id.
    /// </summary>
    public string SpanId { get; init; }

    /// <summary>
    /// Gets the current traceparent value.
    /// </summary>
    public string? TraceParent { get; init; }

    /// <summary>
    /// Gets the current tracestate value.
    /// </summary>
    public string? TraceState { get; init; }

    /// <summary>
    /// Gets the current correlation id when available.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Deconstructs the snapshot into its component values.
    /// </summary>
    /// <param name="traceId">Receives the trace id.</param>
    /// <param name="spanId">Receives the span id.</param>
    /// <param name="traceParent">Receives the traceparent value.</param>
    /// <param name="traceState">Receives the tracestate value.</param>
    /// <param name="correlationId">Receives the correlation id.</param>
    public void Deconstruct(
        out string traceId,
        out string spanId,
        out string? traceParent,
        out string? traceState,
        out string? correlationId)
    {
        traceId = TraceId;
        spanId = SpanId;
        traceParent = TraceParent;
        traceState = TraceState;
        correlationId = CorrelationId;
    }

    /// <summary>
    /// Captures the current activity context.
    /// </summary>
    /// <returns>A snapshot when <see cref="Activity.Current"/> exists; otherwise <see langword="null"/>.</returns>
    public static TraceContextSnapshot? CaptureCurrent()
    {
        var activity = Activity.Current;
        return activity is null ? null : FromActivity(activity);
    }

    /// <summary>
    /// Tries to capture the current trace context.
    /// </summary>
    /// <param name="snapshot">The captured snapshot, when available.</param>
    /// <returns><see langword="true"/> when a current activity exists; otherwise <see langword="false"/>.</returns>
    public static bool TryCaptureCurrent(out TraceContextSnapshot? snapshot)
    {
        snapshot = CaptureCurrent();
        return snapshot is not null;
    }

    /// <summary>
    /// Converts the snapshot into a propagation-friendly header dictionary.
    /// </summary>
    /// <returns>A read-only dictionary of propagation header values.</returns>
    public IReadOnlyDictionary<string, string> ToHeaders()
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        WriteTo(headers);

        return new ReadOnlyDictionary<string, string>(headers);
    }

    /// <summary>
    /// Writes propagation headers into an existing dictionary.
    /// </summary>
    /// <param name="headers">The target header dictionary.</param>
    public void WriteTo(IDictionary<string, string> headers)
    {
        _ = Guard.AgainstNull(headers);

        if (TraceParent is not null)
        {
            headers[TracingHeaderNames.TraceParent] = TraceParent;
        }

        if (TraceState is not null)
        {
            headers[TracingHeaderNames.TraceState] = TraceState;
        }

        if (CorrelationId is not null)
        {
            headers[TracingTagNames.CorrelationId] = CorrelationId;
        }
    }

    /// <summary>
    /// Converts the snapshot to an <see cref="ActivityContext"/> when a valid traceparent value is available.
    /// </summary>
    /// <param name="activityContext">The parsed activity context.</param>
    /// <returns><see langword="true"/> when the snapshot contains a valid traceparent value; otherwise <see langword="false"/>.</returns>
    public bool TryGetActivityContext(out ActivityContext activityContext)
    {
        if (TraceParent is null)
        {
            activityContext = default;
            return false;
        }

        return ActivityContext.TryParse(TraceParent, TraceState, out activityContext);
    }

    /// <summary>
    /// Creates a snapshot from an explicit <see cref="Activity"/>.
    /// </summary>
    /// <param name="activity">The activity.</param>
    /// <returns>A new snapshot instance.</returns>
    public static TraceContextSnapshot FromActivity(Activity activity)
    {
        _ = Guard.AgainstNull(activity);

        var correlationId = activity.GetTagItem(TracingTagNames.CorrelationId)?.ToString();

        return new TraceContextSnapshot(
            activity.TraceId.ToString(),
            activity.SpanId.ToString(),
            activity.Id,
            activity.TraceStateString,
            correlationId);
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
