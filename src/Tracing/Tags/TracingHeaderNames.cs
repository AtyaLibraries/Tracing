// <copyright file="TracingHeaderNames.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
namespace Atya.Diagnostics.Tracing.Tags;

/// <summary>
/// Well-known trace propagation header names used by this package.
/// </summary>
public static class TracingHeaderNames
{
    public const string TraceParent = "traceparent";
    public const string TraceState = "tracestate";
}
