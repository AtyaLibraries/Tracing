using Tracing.UnitTests.TestHelpers;

namespace Tracing.UnitTests.Correlation;

public sealed class CorrelationContextTests
{
    [Fact]
    public void DefaultName_Should_Be_Stable()
    {
        _ = CorrelationHeader.DefaultName.Should().Be("X-Correlation-ID");
    }

    [Fact]
    public void Push_Should_Set_Current_Without_Any_Activity_And_Restore_On_Dispose()
    {
        _ = Activity.Current.Should().BeNull();
        _ = CorrelationContext.Current.Should().BeNull();

        using (CorrelationContext.Push("outer"))
        {
            _ = CorrelationContext.Current.Should().Be("outer");
            _ = Activity.Current.Should().BeNull("no Activity is manufactured");

            using (CorrelationContext.Push("inner"))
            {
                _ = CorrelationContext.Current.Should().Be("inner");
            }

            _ = CorrelationContext.Current.Should().Be("outer");
        }

        _ = CorrelationContext.Current.Should().BeNull();
    }

    [Fact]
    public void Push_Should_Tag_Current_Activity_And_Not_Use_Baggage()
    {
        using var source = new ActivitySource("corr-test");
        using var listener = new ActivityListenerScope("corr-test");
        using var activity = source.StartActivity("op");

        using (CorrelationContext.Push("abc"))
        {
            _ = activity!.GetTagItem(TracingTagNames.CorrelationId).Should().Be("abc");
            _ = activity.Baggage.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Current_Should_Flow_Across_Await()
    {
        using var scope = CorrelationContext.Push("flow");

        var inside = await Task.Run(async () =>
        {
            await Task.Yield();
            return CorrelationContext.Current;
        });

        _ = inside.Should().Be("flow");
    }

    [Fact]
    public void Dispose_Should_Be_Idempotent()
    {
        var outer = CorrelationContext.Push("a");
        var inner = CorrelationContext.Push("b");
        inner.Dispose();
        inner.Dispose();
        _ = CorrelationContext.Current.Should().Be("a");
        outer.Dispose();
        _ = CorrelationContext.Current.Should().BeNull();
    }

    [Fact]
    public void Push_Should_Reject_Blank_Id()
    {
        _ = FluentActions.Invoking(() => CorrelationContext.Push(" ")).Should().Throw<ArgumentException>();
    }
}
