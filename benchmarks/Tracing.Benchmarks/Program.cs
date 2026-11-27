using Atya.Diagnostics.Tracing.Activities;
using Atya.Diagnostics.Tracing.Context;
using Atya.Diagnostics.Tracing.Extensions;
using Atya.Diagnostics.Tracing.Options;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Tracing.Benchmarks;

/// <summary>
/// Runs the Atya.Diagnostics.Tracing benchmark suite.
/// </summary>
public static class Program
{
    /// <summary>
    /// Executes the benchmark suite.
    /// </summary>
    /// <param name="args">Command-line arguments passed to BenchmarkDotNet.</param>
    public static void Main(string[] args)
    {
        _ = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}

/// <summary>
/// Benchmarks activity source access and trace context propagation helpers.
/// </summary>
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

    /// <summary>
    /// Initializes benchmark state.
    /// </summary>
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

    /// <summary>
    /// Cleans up benchmark state.
    /// </summary>
    [GlobalCleanup]
    public void Cleanup()
    {
        this.Dispose();
    }

    /// <summary>
    /// Releases benchmark resources.
    /// </summary>
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

    /// <summary>
    /// Starts an activity when no listener is attached.
    /// </summary>
    /// <returns><see langword="true"/> when no activity is created.</returns>
    [Benchmark(Baseline = true)]
    public bool StartActivityNoListener()
    {
        using var activity = this.noListenerAccessor.StartInternalActivity("benchmarks.no_listener", Tags);
        return activity is null;
    }

    /// <summary>
    /// Starts an activity when a listener is attached.
    /// </summary>
    /// <returns><see langword="true"/> when an activity is created.</returns>
    [Benchmark]
    public bool StartActivityWithListener()
    {
        using var activity = this.listenerAccessor.StartInternalActivity("benchmarks.listener", Tags);
        return activity is not null;
    }

    /// <summary>
    /// Captures the current trace context snapshot.
    /// </summary>
    /// <returns>The current trace context snapshot.</returns>
    [Benchmark]
    public TraceContextSnapshot CaptureCurrentSnapshot()
    {
        ObjectDisposedException.ThrowIf(this.disposed, this);

        return TraceContextSnapshot.CaptureCurrent()!;
    }

    /// <summary>
    /// Creates propagation headers from a trace context snapshot.
    /// </summary>
    /// <returns>The propagation headers.</returns>
    [Benchmark]
    public IReadOnlyDictionary<string, string> CreatePropagationHeaders()
    {
        return this.snapshot.ToHeaders();
    }

    /// <summary>
    /// Writes propagation headers into a dictionary.
    /// </summary>
    /// <returns>The number of written headers.</returns>
    [Benchmark]
    public int WritePropagationHeaders()
    {
        this.headers.Clear();
        this.snapshot.WriteTo(this.headers);
        return this.headers.Count;
    }

    /// <summary>
    /// Parses an activity context from the snapshot.
    /// </summary>
    /// <returns><see langword="true"/> when parsing succeeds.</returns>
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
