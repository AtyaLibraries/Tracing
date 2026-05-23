using Tracing.UnitTests.TestHelpers;

namespace Tracing.UnitTests.Activities;

public sealed class ActivitySourceAccessorTests
{
    [Fact]
    public void Constructor_Should_Create_ActivitySource_With_Configured_Name_And_Version()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new TracingOptions
        {
            ActivitySourceName = "Tests.Tracing",
            ActivitySourceVersion = "1.2.3"
        });

        using var accessor = new ActivitySourceAccessor(options);

        _ = accessor.ActivitySource.Name.Should().Be("Tests.Tracing");
        _ = accessor.ActivitySource.Version.Should().Be("1.2.3");
    }

    [Fact]
    public void Constructor_Should_Throw_When_Options_Is_Null()
    {
        Action act = () => _ = new ActivitySourceAccessor(null!);

        _ = act.Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
    }

    [Fact]
    public void Constructor_Should_Throw_When_ActivitySourceName_Is_Whitespace()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new TracingOptions
        {
            ActivitySourceName = " "
        });

        Action act = () => _ = new ActivitySourceAccessor(options);

        _ = act.Should().Throw<ArgumentException>()
            .WithParameterName("ActivitySourceName");
    }

    [Fact]
    public void Constructor_Should_Throw_When_ServiceName_Is_Whitespace()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new TracingOptions
        {
            ActivitySourceName = "Tests.Tracing",
            ServiceName = " "
        });

        Action act = () => _ = new ActivitySourceAccessor(options);

        _ = act.Should().Throw<ArgumentException>()
            .WithParameterName("ServiceName");
    }

    [Fact]
    public void Constructor_Should_Throw_When_ServiceVersion_Is_Whitespace()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new TracingOptions
        {
            ActivitySourceName = "Tests.Tracing",
            ServiceVersion = " "
        });

        Action act = () => _ = new ActivitySourceAccessor(options);

        _ = act.Should().Throw<ArgumentException>()
            .WithParameterName("ServiceVersion");
    }

    [Fact]
    public void Constructor_Should_Throw_When_DefaultTag_Name_Is_Whitespace()
    {
        var tracingOptions = new TracingOptions
        {
            ActivitySourceName = "Tests.Tracing"
        };
        tracingOptions.DefaultTags[" "] = "invalid";

        var options = Microsoft.Extensions.Options.Options.Create(tracingOptions);

        Action act = () => _ = new ActivitySourceAccessor(options);

        _ = act.Should().Throw<ArgumentException>()
            .WithParameterName("DefaultTags");
    }

    [Fact]
    public void StartActivity_Should_Return_Started_Activity_When_Listener_Is_Registered()
    {
        const string sourceName = "Tests.Tracing";

        using var listenerScope = new ActivityListenerScope(sourceName);
        using var accessor = new ActivitySourceAccessor(Microsoft.Extensions.Options.Options.Create(new TracingOptions
        {
            ActivitySourceName = sourceName
        }));

        var tags = new[]
        {
            new KeyValuePair<string, object?>("operation.name", "ImportCustomers"),
            new KeyValuePair<string, object?>("customer.count", 15)
        };

        using var activity = accessor.StartActivity("ImportCustomers", ActivityKind.Internal, tags: tags);

        _ = activity.Should().NotBeNull();
        _ = activity!.OperationName.Should().Be("ImportCustomers");
        _ = activity.Kind.Should().Be(ActivityKind.Internal);
        _ = activity.GetTagItem("operation.name").Should().Be("ImportCustomers");
        _ = activity.GetTagItem("customer.count").Should().Be(15);
    }

    [Fact]
    public void StartActivity_Should_Return_Null_When_No_Listener_Is_Registered()
    {
        using var accessor = new ActivitySourceAccessor(Microsoft.Extensions.Options.Options.Create(new TracingOptions
        {
            ActivitySourceName = "Tests.Tracing"
        }));

        using var activity = accessor.StartActivity("ImportCustomers");

        _ = activity.Should().BeNull();
    }

    [Fact]
    public void StartActivity_Should_Use_Provided_ParentContext()
    {
        const string sourceName = "Tests.Tracing";

        using var listenerScope = new ActivityListenerScope(sourceName);
        using var accessor = new ActivitySourceAccessor(Microsoft.Extensions.Options.Options.Create(new TracingOptions
        {
            ActivitySourceName = sourceName
        }));

        var traceId = ActivityTraceId.CreateRandom();
        var spanId = ActivitySpanId.CreateRandom();
        var parentContext = new ActivityContext(
            traceId,
            spanId,
            ActivityTraceFlags.Recorded);

        using var activity = accessor.StartActivity(
            "ChildOperation",
            ActivityKind.Internal,
            parentContext);

        _ = activity.Should().NotBeNull();
        _ = activity!.TraceId.Should().Be(traceId);
        _ = activity.ParentSpanId.Should().Be(spanId);
    }

    [Fact]
    public void StartActivity_Should_Include_Links_When_Provided()
    {
        const string sourceName = "Tests.Tracing";

        using var listenerScope = new ActivityListenerScope(sourceName);
        using var accessor = new ActivitySourceAccessor(Microsoft.Extensions.Options.Options.Create(new TracingOptions
        {
            ActivitySourceName = sourceName
        }));

        var linkedContext = new ActivityContext(
            ActivityTraceId.CreateRandom(),
            ActivitySpanId.CreateRandom(),
            ActivityTraceFlags.Recorded);

        var links = new[]
        {
            new ActivityLink(linkedContext)
        };

        using var activity = accessor.StartActivity(
            "ProcessBatch",
            ActivityKind.Internal,
            links: links);

        _ = activity.Should().NotBeNull();
        _ = activity!.Links.Should().ContainSingle();
        _ = activity.Links.Single().Context.TraceId.Should().Be(linkedContext.TraceId);
    }

    [Fact]
    public void StartActivity_Should_Apply_Configured_Service_And_Default_Tags()
    {
        const string sourceName = "Tests.Tracing";

        using var listenerScope = new ActivityListenerScope(sourceName);

        var tracingOptions = new TracingOptions
        {
            ActivitySourceName = sourceName,
            ServiceName = "Orders.Service",
            ServiceVersion = "1.2.3"
        };
        _ = tracingOptions.AddDefaultTag("deployment.environment", "test");
        _ = tracingOptions.AddDefaultTag("optional.empty", null);

        using var accessor = new ActivitySourceAccessor(Microsoft.Extensions.Options.Options.Create(tracingOptions));

        var tags = new[]
        {
            new KeyValuePair<string, object?>(TracingTagNames.ServiceName, "Explicit.Service")
        };

        using var activity = accessor.StartActivity("ProcessOrder", tags: tags);

        _ = activity.Should().NotBeNull();
        _ = activity!.GetTagItem(TracingTagNames.ServiceName).Should().Be("Explicit.Service");
        _ = activity.GetTagItem(TracingTagNames.ServiceVersion).Should().Be("1.2.3");
        _ = activity.GetTagItem("deployment.environment").Should().Be("test");
        _ = activity.GetTagItem("optional.empty").Should().BeNull();
    }

    [Fact]
    public void StartActivity_Should_Use_DefaultTags_Snapshot_From_Construction()
    {
        const string sourceName = "Tests.Tracing";

        using var listenerScope = new ActivityListenerScope(sourceName);

        var tracingOptions = new TracingOptions
        {
            ActivitySourceName = sourceName
        };
        _ = tracingOptions.AddDefaultTag("first", "captured");

        using var accessor = new ActivitySourceAccessor(Microsoft.Extensions.Options.Options.Create(tracingOptions));

        _ = tracingOptions.AddDefaultTag("second", "not-captured");

        using var activity = accessor.StartActivity("ProcessOrder");

        _ = activity.Should().NotBeNull();
        _ = activity!.GetTagItem("first").Should().Be("captured");
        _ = activity.GetTagItem("second").Should().BeNull();
    }

    [Fact]
    public void StartActivity_Should_Throw_When_Name_Is_Whitespace()
    {
        using var accessor = new ActivitySourceAccessor(Microsoft.Extensions.Options.Options.Create(new TracingOptions
        {
            ActivitySourceName = "Tests.Tracing"
        }));

        Action act = () => accessor.StartActivity(" ");

        _ = act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public void StartInternalActivity_Should_Start_Internal_Activity()
    {
        const string sourceName = "Tests.Tracing";

        using var listenerScope = new ActivityListenerScope(sourceName);
        using var accessor = new ActivitySourceAccessor(Microsoft.Extensions.Options.Options.Create(new TracingOptions
        {
            ActivitySourceName = sourceName
        }));

        using var activity = accessor.StartInternalActivity("InternalWork");

        _ = activity.Should().NotBeNull();
        _ = activity!.Kind.Should().Be(ActivityKind.Internal);
    }

    [Fact]
    public void StartClientActivity_Should_Start_Client_Activity()
    {
        const string sourceName = "Tests.Tracing";

        using var listenerScope = new ActivityListenerScope(sourceName);
        using var accessor = new ActivitySourceAccessor(Microsoft.Extensions.Options.Options.Create(new TracingOptions
        {
            ActivitySourceName = sourceName
        }));

        using var activity = accessor.StartClientActivity("CallDependency");

        _ = activity.Should().NotBeNull();
        _ = activity!.Kind.Should().Be(ActivityKind.Client);
    }

    [Fact]
    public void StartServerActivity_Should_Start_Server_Activity()
    {
        const string sourceName = "Tests.Tracing";

        using var listenerScope = new ActivityListenerScope(sourceName);
        using var accessor = new ActivitySourceAccessor(Microsoft.Extensions.Options.Options.Create(new TracingOptions
        {
            ActivitySourceName = sourceName
        }));

        using var activity = accessor.StartServerActivity("HandleRequest");

        _ = activity.Should().NotBeNull();
        _ = activity!.Kind.Should().Be(ActivityKind.Server);
    }

    [Fact]
    public void StartProducerActivity_Should_Start_Producer_Activity()
    {
        const string sourceName = "Tests.Tracing";

        using var listenerScope = new ActivityListenerScope(sourceName);
        using var accessor = new ActivitySourceAccessor(Microsoft.Extensions.Options.Options.Create(new TracingOptions
        {
            ActivitySourceName = sourceName
        }));

        using var activity = accessor.StartProducerActivity("PublishMessage");

        _ = activity.Should().NotBeNull();
        _ = activity!.Kind.Should().Be(ActivityKind.Producer);
    }

    [Fact]
    public void StartConsumerActivity_Should_Start_Consumer_Activity()
    {
        const string sourceName = "Tests.Tracing";

        using var listenerScope = new ActivityListenerScope(sourceName);
        using var accessor = new ActivitySourceAccessor(Microsoft.Extensions.Options.Options.Create(new TracingOptions
        {
            ActivitySourceName = sourceName
        }));

        using var activity = accessor.StartConsumerActivity("ConsumeMessage");

        _ = activity.Should().NotBeNull();
        _ = activity!.Kind.Should().Be(ActivityKind.Consumer);
    }

    [Fact]
    public void Dispose_Should_Prevent_Further_Usage()
    {
        using var accessor = new ActivitySourceAccessor(Microsoft.Extensions.Options.Options.Create(new TracingOptions
        {
            ActivitySourceName = "Tests.Tracing"
        }));

        accessor.Dispose();

        Action act = () => accessor.StartActivity("ImportCustomers");

        _ = act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void Dispose_Should_Be_Idempotent()
    {
        using var accessor = new ActivitySourceAccessor(Microsoft.Extensions.Options.Options.Create(new TracingOptions
        {
            ActivitySourceName = "Tests.Tracing"
        }));

        accessor.Dispose();
        Action act = accessor.Dispose;

        _ = act.Should().NotThrow();
    }
}
