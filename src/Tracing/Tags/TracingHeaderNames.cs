// <copyright file="TracingHeaderNames.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
namespace Atya.Diagnostics.Tracing.Tags;

/// <summary>
/// Well-known trace propagation header names used by this package.
/// </summary>
public static class TracingHeaderNames
{
    /// <summary>
    /// The W3C trace context traceparent header name.
    /// </summary>
    public const string TraceParent = "traceparent";

    /// <summary>
    /// The W3C trace context tracestate header name.
    /// </summary>
    public const string TraceState = "tracestate";
}
