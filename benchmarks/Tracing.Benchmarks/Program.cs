using Atya.Diagnostics.Tracing.Activities;
using Atya.Diagnostics.Tracing.Context;
using Atya.Diagnostics.Tracing.Extensions;
using Atya.Diagnostics.Tracing.Options;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Tracing.Benchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        _ = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}

[MemoryDiagnoser]
public class ActivitySourceAccessorBenchmarks : IDisposable
{
    private static readonly KeyValuePair<string, object?>[] Tags =
    [
        new("operation.name", "ProcessOrder"),
        new("order.id", 12345),
        new("tenant.id", "tenant-benchmark")
    ];

    private ActivitySourceAccessor noListenerAccessor = null!;
    private ActivitySourceAccessor listenerAccessor = null!;
    private ActivityListener listener = null!;
    private Activity currentActivity = null!;
    private TraceContextSnapshot snapshot = null!;
    private Dictionary<string, string> headers = null!;
    private bool disposed;

    [GlobalSetup]
    public void Setup()
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;
        Activity.ForceDefaultIdFormat = true;

        this.noListenerAccessor = CreateAccessor("Benchmarks.Tracing.NoListener");
        this.listenerAccessor = CreateAccessor("Benchmarks.Tracing.Listener");
        this.listener = CreateListener("Benchmarks.Tracing.Listener");

        this.currentActivity = new Activity("benchmarks.current");
        _ = this.currentActivity.Start();
        _ = this.currentActivity.SetCorrelationId("corr-benchmark");

        this.snapshot = TraceContextSnapshot.FromActivity(this.currentActivity);
        this.headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        this.Dispose();
    }

    public void Dispose()
    {
        if (this.disposed)
        {
            return;
        }

        this.currentActivity.Dispose();
        this.listener.Dispose();
        this.listenerAccessor.Dispose();
        this.noListenerAccessor.Dispose();
        this.disposed = true;
        GC.SuppressFinalize(this);
    }

    [Benchmark(Baseline = true)]
    public bool StartActivityNoListener()
    {
        using var activity = this.noListenerAccessor.StartInternalActivity("benchmarks.no_listener", Tags);
        return activity is null;
    }

    [Benchmark]
    public bool StartActivityWithListener()
    {
        using var activity = this.listenerAccessor.StartInternalActivity("benchmarks.listener", Tags);
        return activity is not null;
    }

    [Benchmark]
    public TraceContextSnapshot CaptureCurrentSnapshot()
    {
        ObjectDisposedException.ThrowIf(this.disposed, this);

        return TraceContextSnapshot.CaptureCurrent()!;
    }

    [Benchmark]
    public IReadOnlyDictionary<string, string> CreatePropagationHeaders()
    {
        return this.snapshot.ToHeaders();
    }

    [Benchmark]
    public int WritePropagationHeaders()
    {
        this.headers.Clear();
        this.snapshot.WriteTo(this.headers);
        return this.headers.Count;
    }

    [Benchmark]
    public bool ParseActivityContext()
    {
        return this.snapshot.TryGetActivityContext(out _);
    }

    private static ActivitySourceAccessor CreateAccessor(string sourceName)
    {
        return new ActivitySourceAccessor(Options.Create(new TracingOptions
        {
            ActivitySourceName = sourceName,
            ActivitySourceVersion = "1.0.0",
            ServiceName = "Benchmarks.Service",
            ServiceVersion = "1.0.0"
        }.AddDefaultTag("deployment.environment", "benchmark")));
    }

    private static ActivityListener CreateListener(string sourceName)
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == sourceName,
            Sample = static (ref _) => ActivitySamplingResult.AllDataAndRecorded,
            SampleUsingParentId = static (ref _) => ActivitySamplingResult.AllDataAndRecorded
        };

        ActivitySource.AddActivityListener(listener);
        return listener;
    }
}
