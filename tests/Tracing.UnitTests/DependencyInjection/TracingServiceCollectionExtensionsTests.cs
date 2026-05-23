namespace Tracing.UnitTests.DependencyInjection;

public sealed class TracingServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAtyaTracing_Should_Register_Services()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaTracing(options =>
        {
            options.ActivitySourceName = "Tests.Tracing";
            options.ActivitySourceVersion = "2.0.0";
            options.ServiceName = "Tests.Service";
            options.ServiceVersion = "2.0.0";
            _ = options.AddDefaultTag("deployment.environment", "test");
        });

        using var provider = services.BuildServiceProvider();

        var accessor = provider.GetRequiredService<IActivitySourceAccessor>();
        var options = provider.GetRequiredService<IOptions<TracingOptions>>();

        _ = accessor.Should().NotBeNull();
        _ = accessor.ActivitySource.Name.Should().Be("Tests.Tracing");
        _ = accessor.ActivitySource.Version.Should().Be("2.0.0");
        _ = options.Value.ActivitySourceName.Should().Be("Tests.Tracing");
        _ = options.Value.ActivitySourceVersion.Should().Be("2.0.0");
        _ = options.Value.ServiceName.Should().Be("Tests.Service");
        _ = options.Value.ServiceVersion.Should().Be("2.0.0");
        _ = options.Value.DefaultTags.Should().Contain("deployment.environment", "test");
    }

    [Fact]
    public void AddAtyaTracing_Should_Register_Accessor_As_Singleton()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaTracing(options =>
        {
            options.ActivitySourceName = "Tests.Tracing";
        });

        using var provider = services.BuildServiceProvider();

        var first = provider.GetRequiredService<IActivitySourceAccessor>();
        var second = provider.GetRequiredService<IActivitySourceAccessor>();

        _ = first.Should().BeSameAs(second);
    }

    [Fact]
    public void AddAtyaTracing_Should_Throw_When_Services_Is_Null()
    {
        ServiceCollection? services = null;

        Action act = () => services!.AddAtyaTracing();

        _ = act.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }

    [Fact]
    public void AddAtyaTracing_Should_Allow_Default_Options()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaTracing();

        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<TracingOptions>>();
        var accessor = provider.GetRequiredService<IActivitySourceAccessor>();

        _ = options.Value.ActivitySourceName.Should().Be("Atya");
        _ = options.Value.ActivitySourceVersion.Should().BeNull();
        _ = options.Value.ServiceName.Should().BeNull();
        _ = options.Value.ServiceVersion.Should().BeNull();
        _ = options.Value.DefaultTags.Should().BeEmpty();
        _ = accessor.ActivitySource.Name.Should().Be("Atya");
    }

    [Fact]
    public void AddAtyaTracing_Should_Validate_Options_When_Resolved()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaTracing(options =>
        {
            options.ActivitySourceName = " ";
        });

        using var provider = services.BuildServiceProvider();

        Action act = () => _ = provider.GetRequiredService<IActivitySourceAccessor>();

        _ = act.Should().Throw<OptionsValidationException>()
            .Which.Failures.Should().Contain("TracingOptions.ActivitySourceName cannot be null or whitespace.");
    }

    [Fact]
    public void AddAtyaTracing_Should_Validate_Service_Options_When_Resolved()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaTracing(options =>
        {
            options.ServiceName = " ";
            options.ServiceVersion = " ";
            options.DefaultTags[" "] = "invalid";
        });

        using var provider = services.BuildServiceProvider();

        Action act = () => _ = provider.GetRequiredService<IActivitySourceAccessor>();

        var exception = act.Should().Throw<OptionsValidationException>().Which;
        _ = exception.Failures.Should().Contain("TracingOptions.ServiceName cannot be whitespace.");
        _ = exception.Failures.Should().Contain("TracingOptions.ServiceVersion cannot be whitespace.");
        _ = exception.Failures.Should().Contain("TracingOptions.DefaultTags cannot contain null, empty, or whitespace tag names.");
    }
}
