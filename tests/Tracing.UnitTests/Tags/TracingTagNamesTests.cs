namespace Tracing.UnitTests.Tags;

public sealed class TracingTagNamesTests
{
    [Fact]
    public void Tag_Names_Should_Match_Expected_Values()
    {
        _ = TracingTagNames.CorrelationId.Should().Be("correlation.id");
        _ = TracingTagNames.TenantId.Should().Be("tenant.id");
        _ = TracingTagNames.UserId.Should().Be("user.id");
        _ = TracingTagNames.OperationName.Should().Be("operation.name");
        _ = TracingTagNames.EntityType.Should().Be("entity.type");
        _ = TracingTagNames.EntityId.Should().Be("entity.id");
        _ = TracingTagNames.Outcome.Should().Be("outcome");
        _ = TracingTagNames.ErrorType.Should().Be("error.type");
        _ = TracingTagNames.ErrorMessage.Should().Be("error.message");
        _ = TracingTagNames.DependencySystem.Should().Be("dependency.system");
        _ = TracingTagNames.MessagingDestination.Should().Be("messaging.destination");
        _ = TracingTagNames.ServiceName.Should().Be("service.name");
        _ = TracingTagNames.ServiceVersion.Should().Be("service.version");
    }

    [Fact]
    public void Header_Names_Should_Match_Expected_Values()
    {
        _ = TracingHeaderNames.TraceParent.Should().Be("traceparent");
        _ = TracingHeaderNames.TraceState.Should().Be("tracestate");
    }
}
