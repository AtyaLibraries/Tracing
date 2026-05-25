using System.Diagnostics;
using Atya.Diagnostics.Tracing.Abstractions;
using Atya.Diagnostics.Tracing.Context;
using Atya.Diagnostics.Tracing.Extensions;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddAtyaTracing(options =>
{
    options.ActivitySourceName = "Samples.Tracing";
    options.ActivitySourceVersion = "1.0.0";
    options.ServiceName = "Samples.Console";
    options.ServiceVersion = "1.0.0";
    _ = options.AddDefaultTag("deployment.environment", "local");
});

using var serviceProvider = services.BuildServiceProvider();
var activitySourceAccessor = serviceProvider.GetRequiredService<IActivitySourceAccessor>();

using var listener = CreateConsoleListener("Samples.Tracing");

Console.WriteLine("=== Atya.Diagnostics.Tracing sample ===");
Console.WriteLine();

RunInternalOperation(activitySourceAccessor);
RunClientOperation(activitySourceAccessor);
RunProducerConsumerOperation(activitySourceAccessor);
RunFailureOperation(activitySourceAccessor);

Console.WriteLine();
Console.WriteLine("Finished.");

static void RunInternalOperation(IActivitySourceAccessor activitySourceAccessor)
{
    Console.WriteLine("1. Internal operation");
    Console.WriteLine();

    using var activity = activitySourceAccessor.StartInternalActivity(
        "orders.import",
        new[]
        {
            new KeyValuePair<string, object?>("orders.count", 25),
            new KeyValuePair<string, object?>("source.system", "crm")
        });

    _ = (activity?
        .SetCorrelationId(Guid.NewGuid().ToString("N"))
        .SetTenantId("tenant-a")
        .SetUserId("user-42")
        .SetOperationName("ImportOrders")
        .SetEntity("OrderBatch", "batch-20260322")
        .SetOutcome("started"));

    var snapshot = TraceContextSnapshot.CaptureCurrent();

    Console.WriteLine($"TraceId: {snapshot?.TraceId}");
    Console.WriteLine($"SpanId: {snapshot?.SpanId}");
    Console.WriteLine($"TraceParent: {snapshot?.TraceParent}");
    Console.WriteLine();

    if (snapshot is not null)
    {
        Console.WriteLine("Propagation headers:");
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        snapshot.WriteTo(headers);

        foreach (var header in headers)
        {
            Console.WriteLine($"{header.Key}: {header.Value}");
        }

        Console.WriteLine();
    }

    _ = (activity?.MarkSuccess());

    Console.WriteLine();
}

static void RunClientOperation(IActivitySourceAccessor activitySourceAccessor)
{
    Console.WriteLine("2. Client operation");
    Console.WriteLine();

    using var activity = activitySourceAccessor.StartClientActivity("inventory.http.request");

    _ = (activity?
        .SetCorrelationId(Guid.NewGuid().ToString("N"))
        .SetDependencySystem("http")
        .SetOperationName("GetInventory")
        .SetTagIfNotNull("http.method", "GET")
        .SetTagIfNotNull("http.route", "/api/inventory/42")
        .SetTagIfNotNull("server.address", "inventory-service"));

    SimulateWork(50);

    _ = (activity?.MarkSuccess());

    Console.WriteLine();
}

static void RunProducerConsumerOperation(IActivitySourceAccessor activitySourceAccessor)
{
    Console.WriteLine("3. Producer and consumer operation");
    Console.WriteLine();

    TraceContextSnapshot? producedSnapshot;

    using (var producerActivity = activitySourceAccessor.StartProducerActivity("orders.publish"))
    {
        _ = (producerActivity?
            .SetCorrelationId(Guid.NewGuid().ToString("N"))
            .SetMessagingDestination("orders.created")
            .SetOperationName("PublishOrderCreated")
            .SetEntity("Order", 1001));

        SimulateWork(20);

        producedSnapshot = TraceContextSnapshot.CaptureCurrent();

        _ = (producerActivity?.MarkSuccess());
    }

    if (producedSnapshot is null)
    {
        Console.WriteLine("Producer snapshot was not captured.");
        Console.WriteLine();
        return;
    }

    if (!producedSnapshot.TryGetActivityContext(out var parentContext))
    {
        Console.WriteLine("Producer snapshot did not contain a valid trace context.");
        Console.WriteLine();
        return;
    }

    using var consumerActivity = activitySourceAccessor.StartActivity(
        "orders.consume",
        ActivityKind.Consumer,
        parentContext);

    _ = (consumerActivity?
        .SetCorrelationId(producedSnapshot.CorrelationId ?? Guid.NewGuid().ToString("N"))
        .SetMessagingDestination("orders.created")
        .SetOperationName("ConsumeOrderCreated")
        .SetEntity("Order", 1001));

    SimulateWork(20);

    _ = (consumerActivity?.MarkSuccess());

    Console.WriteLine();
}

static void RunFailureOperation(IActivitySourceAccessor activitySourceAccessor)
{
    Console.WriteLine("4. Failure operation");
    Console.WriteLine();

    using var activity = activitySourceAccessor.StartServerActivity("payments.handle");

    try
    {
        _ = (activity?
            .SetCorrelationId(Guid.NewGuid().ToString("N"))
            .SetOperationName("HandlePayment")
            .SetEntity("Payment", 501)
            .SetTagIfNotNull("payment.amount", 150.75m)
            .SetTagIfNotNull("payment.currency", "USD"));

        SimulateFailure();
        _ = (activity?.MarkSuccess());
    }
    catch (Exception exception)
    {
        _ = (activity?.MarkError(exception));
        Console.WriteLine($"Handled exception: {exception.Message}");
    }

    Console.WriteLine();
}

static ActivityListener CreateConsoleListener(string sourceName)
{
    var listener = new ActivityListener
    {
        ShouldListenTo = source => source.Name == sourceName,
        Sample = static (ref _) => ActivitySamplingResult.AllDataAndRecorded,
        SampleUsingParentId = static (ref _) => ActivitySamplingResult.AllDataAndRecorded,
        ActivityStarted = static activity =>
        {
            Console.WriteLine($"[START] {activity.DisplayName}");
            Console.WriteLine($"  Kind: {activity.Kind}");
            Console.WriteLine($"  TraceId: {activity.TraceId}");
            Console.WriteLine($"  SpanId: {activity.SpanId}");

            if (activity.ParentSpanId != default)
            {
                Console.WriteLine($"  ParentSpanId: {activity.ParentSpanId}");
            }

            foreach (var tag in activity.Tags)
            {
                Console.WriteLine($"  Tag: {tag.Key} = {tag.Value}");
            }

            Console.WriteLine();
        },
        ActivityStopped = static activity =>
        {
            Console.WriteLine($"[STOP] {activity.DisplayName}");
            Console.WriteLine($"  Status: {activity.Status}");

            foreach (var tag in activity.Tags)
            {
                Console.WriteLine($"  Tag: {tag.Key} = {tag.Value}");
            }

            Console.WriteLine();
        }
    };

    ActivitySource.AddActivityListener(listener);
    return listener;
}

static void SimulateWork(int milliseconds)
{
    Thread.Sleep(milliseconds);
}

static void SimulateFailure()
{
    throw new InvalidOperationException("Payment provider returned an unexpected response.");
}
