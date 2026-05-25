# Atya.Diagnostics.Tracing

Provider-agnostic tracing helpers for .NET applications built on `System.Diagnostics.Activity`.

## Installation

```bash
dotnet add package Atya.Diagnostics.Tracing
```

## What This Package Provides

- DI registration for a package-owned `ActivitySource`.
- Convenience methods for internal, client, server, producer, and consumer activities.
- Common tag helpers for correlation, tenant, user, entity, dependency, messaging, outcomes, and errors.
- Optional service/default tags applied to package-created activities.
- Lightweight trace-context snapshots and W3C propagation header helpers.

This package intentionally does not configure OpenTelemetry SDK exporters, logging providers, ASP.NET Core middleware, or vendor-specific integrations.

## Quick Start

```csharp
using Atya.Diagnostics.Tracing.Abstractions;
using Atya.Diagnostics.Tracing.Extensions;

services.AddAtyaTracing(options =>
{
    options.ActivitySourceName = "Orders.Service";
    options.ActivitySourceVersion = "1.0.0";
    options.ServiceName = "Orders.Service";
    options.ServiceVersion = "1.0.0";
    _ = options.AddDefaultTag("deployment.environment", "production");
});

public sealed class OrderProcessor(IActivitySourceAccessor activitySource)
{
    public void Process(int orderId)
    {
        using var activity = activitySource.StartInternalActivity("orders.process");

        _ = activity?
            .SetOperationName("ProcessOrder")
            .SetEntity("Order", orderId)
            .MarkSuccess();
    }
}
```

## Propagation

```csharp
var snapshot = TraceContextSnapshot.CaptureCurrent();

if (snapshot is not null)
{
    var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    snapshot.WriteTo(headers);
}
```

Use `snapshot.TryGetActivityContext(out var context)` to recreate an `ActivityContext` from a captured W3C `traceparent` value.

## Error Handling

`MarkError(Exception)` sets `ActivityStatusCode.Error`, `outcome=error`, `error.type`, and `error.message` when an exception has a non-empty message. `MarkSuccess()` sets `ActivityStatusCode.Ok` and `outcome=success`.

## Compatibility

This package targets `net10.0`.
