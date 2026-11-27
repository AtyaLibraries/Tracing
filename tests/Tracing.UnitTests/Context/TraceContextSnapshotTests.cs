namespace Tracing.UnitTests.Context;

public sealed class TraceContextSnapshotTests
{
    [Fact]
    public void Constructor_Should_Normalize_Optional_Whitespace_Values()
    {
        var snapshot = new TraceContextSnapshot(
            traceId: ActivityTraceId.CreateRandom().ToString(),
            spanId: ActivitySpanId.CreateRandom().ToString(),
            traceParent: " ",
            traceState: " ",
            correlationId: " ");

        _ = snapshot.TraceParent.Should().BeNull();
        _ = snapshot.TraceState.Should().BeNull();
        _ = snapshot.CorrelationId.Should().BeNull();
    }

    [Theory]
    [InlineData(null, "spanId", "traceId")]
    [InlineData(" ", "spanId", "traceId")]
    [InlineData("traceId", null, "spanId")]
    [InlineData("traceId", " ", "spanId")]
    public void Constructor_Should_Throw_When_Required_Values_Are_Invalid(
        string? traceId,
        string? spanId,
        string expectedParameter)
    {
        Action act = () => _ = new TraceContextSnapshot(
            traceId!,
            spanId!,
            traceParent: null,
            traceState: null,
            correlationId: null);

        _ = act.Should().Throw<ArgumentException>()
            .WithParameterName(expectedParameter);
    }

    [Fact]
    public void Deconstruct_Should_Return_All_Values()
    {
        var snapshot = new TraceContextSnapshot(
            traceId: "trace-id",
            spanId: "span-id",
            traceParent: "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
            traceState: "vendor=state",
            correlationId: "corr-123");

        var (traceId, spanId, traceParent, traceState, correlationId) = snapshot;

        _ = traceId.Should().Be("trace-id");
        _ = spanId.Should().Be("span-id");
        _ = traceParent.Should().Be("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01");
        _ = traceState.Should().Be("vendor=state");
        _ = correlationId.Should().Be("corr-123");
    }

    [Fact]
    public void CaptureCurrent_Should_Return_Null_When_No_Current_Activity()
    {
        var previous = Activity.Current;
        Activity.Current = null;

        try
        {
            var snapshot = TraceContextSnapshot.CaptureCurrent();

            _ = snapshot.Should().BeNull();
        }
        finally
        {
            Activity.Current = previous;
        }
    }

    [Fact]
    public void CaptureCurrent_Should_Return_Snapshot_When_Current_Activity_Exists()
    {
        using var activity = new Activity("TestActivity");
        _ = activity.Start();
        _ = activity.SetCorrelationId("corr-123");

        var snapshot = TraceContextSnapshot.CaptureCurrent();

        _ = snapshot.Should().NotBeNull();
        _ = snapshot!.TraceId.Should().Be(activity.TraceId.ToString());
        _ = snapshot.SpanId.Should().Be(activity.SpanId.ToString());
        _ = snapshot.TraceParent.Should().Be(activity.Id);
        _ = snapshot.TraceState.Should().Be(activity.TraceStateString);
        _ = snapshot.CorrelationId.Should().Be("corr-123");
    }

    [Fact]
    public void TryCaptureCurrent_Should_Return_False_When_No_Current_Activity()
    {
        var previous = Activity.Current;
        Activity.Current = null;

        try
        {
            var result = TraceContextSnapshot.TryCaptureCurrent(out var snapshot);

            _ = result.Should().BeFalse();
            _ = snapshot.Should().BeNull();
        }
        finally
        {
            Activity.Current = previous;
        }
    }

    [Fact]
    public void TryCaptureCurrent_Should_Return_True_When_Current_Activity_Exists()
    {
        using var activity = new Activity("TestActivity");
        _ = activity.Start();

        var result = TraceContextSnapshot.TryCaptureCurrent(out var snapshot);

        _ = result.Should().BeTrue();
        _ = snapshot.Should().NotBeNull();
        _ = snapshot!.TraceId.Should().Be(activity.TraceId.ToString());
    }

    [Fact]
    public void ToHeaders_Should_Include_TraceParent_When_Available()
    {
        var snapshot = new TraceContextSnapshot(
            traceId: ActivityTraceId.CreateRandom().ToString(),
            spanId: ActivitySpanId.CreateRandom().ToString(),
            traceParent: "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
            traceState: null,
            correlationId: null);

        var headers = snapshot.ToHeaders();

        _ = headers.Should().ContainKey("traceparent");
        _ = headers["traceparent"].Should().Be("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01");
    }

    [Fact]
    public void ToHeaders_Should_Include_TraceState_And_CorrelationId_When_Available()
    {
        var snapshot = new TraceContextSnapshot(
            traceId: ActivityTraceId.CreateRandom().ToString(),
            spanId: ActivitySpanId.CreateRandom().ToString(),
            traceParent: "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
            traceState: "rojo=00f067aa0ba902b7",
            correlationId: "corr-123");

        var headers = snapshot.ToHeaders();

        _ = headers.Should().ContainKey("traceparent");
        _ = headers.Should().ContainKey("tracestate");
        _ = headers.Should().ContainKey(CorrelationHeader.DefaultName);

        _ = headers["tracestate"].Should().Be("rojo=00f067aa0ba902b7");
        _ = headers[CorrelationHeader.DefaultName].Should().Be("corr-123");
    }

    [Fact]
    public void WriteTo_Should_Write_Propagation_Headers_To_Existing_Dictionary()
    {
        var snapshot = new TraceContextSnapshot(
            traceId: ActivityTraceId.CreateRandom().ToString(),
            spanId: ActivitySpanId.CreateRandom().ToString(),
            traceParent: "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
            traceState: "rojo=00f067aa0ba902b7",
            correlationId: "corr-123");

        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["existing"] = "value"
        };

        snapshot.WriteTo(headers);

        _ = headers.Should().Contain("existing", "value");
        _ = headers.Should().Contain(TracingHeaderNames.TraceParent, "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01");
        _ = headers.Should().Contain(TracingHeaderNames.TraceState, "rojo=00f067aa0ba902b7");
        _ = headers.Should().Contain(CorrelationHeader.DefaultName, "corr-123");
    }

    [Fact]
    public void WriteTo_Should_Throw_When_Headers_Is_Null()
    {
        var snapshot = new TraceContextSnapshot(
            traceId: ActivityTraceId.CreateRandom().ToString(),
            spanId: ActivitySpanId.CreateRandom().ToString(),
            traceParent: null,
            traceState: null,
            correlationId: null);

        Action act = () => snapshot.WriteTo(null!);

        _ = act.Should().Throw<ArgumentNullException>()
            .WithParameterName("headers");
    }

    [Fact]
    public void TryGetActivityContext_Should_Return_True_When_TraceParent_Is_Valid()
    {
        var traceId = ActivityTraceId.CreateRandom();
        var spanId = ActivitySpanId.CreateRandom();
        var snapshot = new TraceContextSnapshot(
            traceId: traceId.ToString(),
            spanId: spanId.ToString(),
            traceParent: $"00-{traceId}-{spanId}-01",
            traceState: "rojo=00f067aa0ba902b7",
            correlationId: null);

        var result = snapshot.TryGetActivityContext(out var activityContext);

        _ = result.Should().BeTrue();
        _ = activityContext.TraceId.Should().Be(traceId);
        _ = activityContext.SpanId.Should().Be(spanId);
        _ = activityContext.TraceFlags.Should().Be(ActivityTraceFlags.Recorded);
    }

    [Fact]
    public void TryGetActivityContext_Should_Return_False_When_TraceParent_Is_Missing()
    {
        var snapshot = new TraceContextSnapshot(
            traceId: ActivityTraceId.CreateRandom().ToString(),
            spanId: ActivitySpanId.CreateRandom().ToString(),
            traceParent: null,
            traceState: null,
            correlationId: null);

        var result = snapshot.TryGetActivityContext(out var activityContext);

        _ = result.Should().BeFalse();
        _ = activityContext.Should().Be(default(ActivityContext));
    }

    [Fact]
    public void TryGetActivityContext_Should_Return_False_When_TraceParent_Is_Invalid()
    {
        var snapshot = new TraceContextSnapshot(
            traceId: ActivityTraceId.CreateRandom().ToString(),
            spanId: ActivitySpanId.CreateRandom().ToString(),
            traceParent: "invalid",
            traceState: null,
            correlationId: null);

        var result = snapshot.TryGetActivityContext(out var activityContext);

        _ = result.Should().BeFalse();
        _ = activityContext.Should().Be(default(ActivityContext));
    }

    [Fact]
    public void FromActivity_Should_Create_Snapshot_From_Provided_Activity()
    {
        using var activity = new Activity("TestActivity");
        _ = activity.Start();
        _ = activity.SetCorrelationId("corr-999");

        var snapshot = TraceContextSnapshot.FromActivity(activity);

        _ = snapshot.TraceId.Should().Be(activity.TraceId.ToString());
        _ = snapshot.SpanId.Should().Be(activity.SpanId.ToString());
        _ = snapshot.TraceParent.Should().Be(activity.Id);
        _ = snapshot.CorrelationId.Should().Be("corr-999");
    }

    [Fact]
    public void FromActivity_Should_Throw_When_Activity_Is_Null()
    {
        Action act = () => _ = TraceContextSnapshot.FromActivity(null!);

        _ = act.Should().Throw<ArgumentNullException>()
            .WithParameterName("activity");
    }

    [Fact]
    public void Constructor_Should_Drop_Malformed_TraceParent()
    {
        var snapshot = new TraceContextSnapshot("trace", "span", "not-a-traceparent", null, null);

        _ = snapshot.TraceParent.Should().BeNull();
        _ = snapshot.ToHeaders().Should().NotContainKey(TracingHeaderNames.TraceParent);
    }

    [Fact]
    public void With_Expression_Should_Not_Bypass_TraceParent_Validation()
    {
        var snapshot = new TraceContextSnapshot("trace", "span", null, null, null);

        var changed = snapshot with { TraceParent = "junk", TraceState = new string('a', 600) };

        _ = changed.TraceParent.Should().BeNull();
        _ = changed.TraceState.Should().BeNull();
    }

    [Fact]
    public void Constructor_Should_Drop_Oversized_Or_ControlCharacter_TraceState()
    {
        var oversized = new TraceContextSnapshot("trace", "span", null, new string('a', 513), null);
        var control = new TraceContextSnapshot("trace", "span", null, "vendor=a\r\nX-Injected: 1", null);
        var valid = new TraceContextSnapshot("trace", "span", null, "vendor=state", null);

        _ = oversized.TraceState.Should().BeNull();
        _ = control.TraceState.Should().BeNull();
        _ = valid.TraceState.Should().Be("vendor=state");
    }

    [Fact]
    public void FromActivity_Should_Not_Treat_A_Hierarchical_Activity_Id_As_TraceParent()
    {
        var previous = Activity.DefaultIdFormat;
        var previousForce = Activity.ForceDefaultIdFormat;
        try
        {
            Activity.DefaultIdFormat = ActivityIdFormat.Hierarchical;
            Activity.ForceDefaultIdFormat = true;
            using var activity = new Activity("Hierarchical");
            _ = activity.Start();

            var snapshot = TraceContextSnapshot.FromActivity(activity);

            _ = activity.Id.Should().StartWith("|");
            _ = snapshot.TraceParent.Should().BeNull();
        }
        finally
        {
            Activity.DefaultIdFormat = previous;
            Activity.ForceDefaultIdFormat = previousForce;
        }
    }
}
