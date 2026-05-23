namespace Tracing.UnitTests.Options;

public sealed class TracingOptionsTests
{
    [Fact]
    public void TracingOptions_Should_Have_Expected_Defaults()
    {
        var options = new TracingOptions();

        _ = options.ActivitySourceName.Should().Be("Atya");
        _ = options.ActivitySourceVersion.Should().BeNull();
        _ = options.ServiceName.Should().BeNull();
        _ = options.ServiceVersion.Should().BeNull();
        _ = options.DefaultTags.Should().BeEmpty();
    }

    [Fact]
    public void AddDefaultTag_Should_Add_Tag_And_Return_Same_Instance()
    {
        var options = new TracingOptions();

        var returned = options.AddDefaultTag("deployment.environment", "test");

        _ = returned.Should().BeSameAs(options);
        _ = options.DefaultTags.Should().Contain("deployment.environment", "test");
    }

    [Fact]
    public void AddDefaultTag_Should_Replace_Existing_Tag()
    {
        var options = new TracingOptions();

        _ = options.AddDefaultTag("deployment.environment", "test");
        _ = options.AddDefaultTag("deployment.environment", "prod");

        _ = options.DefaultTags.Should().Contain("deployment.environment", "prod");
    }

    [Fact]
    public void AddDefaultTag_Should_Throw_When_Name_Is_Whitespace()
    {
        var options = new TracingOptions();

        Action act = () => options.AddDefaultTag(" ", "test");

        _ = act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }
}
