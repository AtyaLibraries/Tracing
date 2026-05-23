namespace Tracing.UnitTests.Extensions;

public sealed class ActivityExtensionsTests
{
    [Fact]
    public void SetTagIfNotNull_Should_Set_Tag_When_Value_Is_Not_Null()
    {
        using var activity = new Activity("TestActivity");
        _ = activity.Start();

        _ = activity.SetTagIfNotNull("customer.id", 42);

        _ = activity.GetTagItem("customer.id").Should().Be(42);
    }

    [Fact]
    public void SetTagIfNotNull_Should_Not_Set_Tag_When_Value_Is_Null()
    {
        using var activity = new Activity("TestActivity");
        _ = activity.Start();

        _ = activity.SetTagIfNotNull("customer.id", null);

        _ = activity.GetTagItem("customer.id").Should().BeNull();
    }

    [Fact]
    public void SetTagIfNotNull_Should_Throw_When_Activity_Is_Null()
    {
        Activity? activity = null;

        Action act = () => activity!.SetTagIfNotNull("customer.id", 1);

        _ = act.Should().Throw<ArgumentNullException>()
            .WithParameterName("activity");
    }

    [Fact]
    public void SetTagIfNotNull_Should_Throw_When_Tag_Name_Is_Whitespace()
    {
        using var activity = new Activity("TestActivity");
        _ = activity.Start();

        Action act = () => activity.SetTagIfNotNull(" ", 1);

        _ = act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public void SetCorrelationId_Should_Set_Correlation_Tag()
    {
        using var activity = new Activity("TestActivity");
        _ = activity.Start();

        _ = activity.SetCorrelationId("corr-123");

        _ = activity.GetTagItem(TracingTagNames.CorrelationId).Should().Be("corr-123");
    }

    [Fact]
    public void SetCorrelationId_Should_Throw_When_CorrelationId_Is_Whitespace()
    {
        using var activity = new Activity("TestActivity");
        _ = activity.Start();

        Action act = () => activity.SetCorrelationId(" ");

        _ = act.Should().Throw<ArgumentException>()
            .WithParameterName("correlationId");
    }

    [Fact]
    public void SetTenantId_Should_Set_Tenant_Tag()
    {
        using var activity = new Activity("TestActivity");
        _ = activity.Start();

        _ = activity.SetTenantId("tenant-1");

        _ = activity.GetTagItem(TracingTagNames.TenantId).Should().Be("tenant-1");
    }

    [Fact]
    public void SetUserId_Should_Set_User_Tag()
    {
        using var activity = new Activity("TestActivity");
        _ = activity.Start();

        _ = activity.SetUserId("user-1");

        _ = activity.GetTagItem(TracingTagNames.UserId).Should().Be("user-1");
    }

    [Fact]
    public void SetOperationName_Should_Set_Operation_Tag()
    {
        using var activity = new Activity("TestActivity");
        _ = activity.Start();

        _ = activity.SetOperationName("ImportCustomers");

        _ = activity.GetTagItem(TracingTagNames.OperationName).Should().Be("ImportCustomers");
    }

    [Fact]
    public void SetEntity_Should_Set_Entity_Type_And_Id()
    {
        using var activity = new Activity("TestActivity");
        _ = activity.Start();

        _ = activity.SetEntity("Order", 99);

        _ = activity.GetTagItem(TracingTagNames.EntityType).Should().Be("Order");
        _ = activity.GetTagItem(TracingTagNames.EntityId).Should().Be(99);
    }

    [Fact]
    public void SetEntity_Should_Set_Only_Entity_Type_When_EntityId_Is_Null()
    {
        using var activity = new Activity("TestActivity");
        _ = activity.Start();

        _ = activity.SetEntity("Order", null);

        _ = activity.GetTagItem(TracingTagNames.EntityType).Should().Be("Order");
        _ = activity.GetTagItem(TracingTagNames.EntityId).Should().BeNull();
    }

    [Fact]
    public void SetEntity_Should_Throw_When_EntityType_Is_Whitespace()
    {
        using var activity = new Activity("TestActivity");
        _ = activity.Start();

        Action act = () => activity.SetEntity(" ", 99);

        _ = act.Should().Throw<ArgumentException>()
            .WithParameterName("entityType");
    }

    [Fact]
    public void SetDependencySystem_Should_Set_Dependency_System_Tag()
    {
        using var activity = new Activity("TestActivity");
        _ = activity.Start();

        _ = activity.SetDependencySystem("sqlserver");

        _ = activity.GetTagItem(TracingTagNames.DependencySystem).Should().Be("sqlserver");
    }

    [Fact]
    public void SetMessagingDestination_Should_Set_Messaging_Destination_Tag()
    {
        using var activity = new Activity("TestActivity");
        _ = activity.Start();

        _ = activity.SetMessagingDestination("orders-topic");

        _ = activity.GetTagItem(TracingTagNames.MessagingDestination).Should().Be("orders-topic");
    }

    [Fact]
    public void MarkSuccess_Should_Set_Status_And_Outcome()
    {
        using var activity = new Activity("TestActivity");
        _ = activity.Start();

        _ = activity.MarkSuccess();

        _ = activity.Status.Should().Be(ActivityStatusCode.Ok);
        _ = activity.GetTagItem(TracingTagNames.Outcome).Should().Be("success");
    }

    [Fact]
    public void MarkError_With_Exception_Should_Set_Status_And_Error_Tags()
    {
        using var activity = new Activity("TestActivity");
        _ = activity.Start();

        var exception = new InvalidOperationException("Something failed.");

        _ = activity.MarkError(exception);

        _ = activity.Status.Should().Be(ActivityStatusCode.Error);
        _ = activity.GetTagItem(TracingTagNames.Outcome).Should().Be("error");
        _ = activity.GetTagItem(TracingTagNames.ErrorType).Should().Be(typeof(InvalidOperationException).FullName);
        _ = activity.GetTagItem(TracingTagNames.ErrorMessage).Should().Be("Something failed.");
    }

    [Fact]
    public void MarkError_With_Exception_Should_Not_Set_ErrorMessage_When_Message_Is_Empty()
    {
        using var activity = new Activity("TestActivity");
        _ = activity.Start();

        var exception = new InvalidOperationException(string.Empty);

        _ = activity.MarkError(exception);

        _ = activity.Status.Should().Be(ActivityStatusCode.Error);
        _ = activity.GetTagItem(TracingTagNames.ErrorType).Should().Be(typeof(InvalidOperationException).FullName);
        _ = activity.GetTagItem(TracingTagNames.ErrorMessage).Should().BeNull();
    }

    [Fact]
    public void MarkError_With_Exception_Should_Throw_When_Exception_Is_Null()
    {
        using var activity = new Activity("TestActivity");
        _ = activity.Start();

        Action act = () => activity.MarkError((Exception)null!);

        _ = act.Should().Throw<ArgumentNullException>()
            .WithParameterName("exception");
    }

    [Fact]
    public void MarkError_With_ErrorType_Should_Set_Status_And_Error_Tags()
    {
        using var activity = new Activity("TestActivity");
        _ = activity.Start();

        _ = activity.MarkError("validation", "Validation failed.");

        _ = activity.Status.Should().Be(ActivityStatusCode.Error);
        _ = activity.GetTagItem(TracingTagNames.Outcome).Should().Be("error");
        _ = activity.GetTagItem(TracingTagNames.ErrorType).Should().Be("validation");
        _ = activity.GetTagItem(TracingTagNames.ErrorMessage).Should().Be("Validation failed.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    public void MarkError_With_ErrorType_Should_Not_Set_ErrorMessage_When_Message_Is_Missing(string? message)
    {
        using var activity = new Activity("TestActivity");
        _ = activity.Start();

        _ = activity.MarkError("validation", message);

        _ = activity.Status.Should().Be(ActivityStatusCode.Error);
        _ = activity.GetTagItem(TracingTagNames.ErrorType).Should().Be("validation");
        _ = activity.GetTagItem(TracingTagNames.ErrorMessage).Should().BeNull();
    }

    [Fact]
    public void MarkError_With_ErrorType_Should_Throw_When_ErrorType_Is_Whitespace()
    {
        using var activity = new Activity("TestActivity");
        _ = activity.Start();

        Action act = () => activity.MarkError(" ");

        _ = act.Should().Throw<ArgumentException>()
            .WithParameterName("errorType");
    }

    [Fact]
    public void SetOutcome_Should_Set_Custom_Outcome_Tag()
    {
        using var activity = new Activity("TestActivity");
        _ = activity.Start();

        _ = activity.SetOutcome("partial-success");

        _ = activity.GetTagItem(TracingTagNames.Outcome).Should().Be("partial-success");
    }

    [Fact]
    public void SetCorrelationId_Should_Return_Same_Activity()
    {
        using var activity = new Activity("TestActivity");
        _ = activity.Start();

        var returned = activity.SetCorrelationId("corr-123");

        _ = returned.Should().BeSameAs(activity);
    }
}
