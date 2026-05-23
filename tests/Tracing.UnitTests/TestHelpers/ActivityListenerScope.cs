namespace Tracing.UnitTests.TestHelpers;

internal sealed class ActivityListenerScope : IDisposable
{
    private readonly ActivityListener listener;
    private bool disposed;

    public ActivityListenerScope(string activitySourceName)
    {
        listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == activitySourceName,
            Sample = static (ref _) => ActivitySamplingResult.AllDataAndRecorded,
            SampleUsingParentId = static (ref _) => ActivitySamplingResult.AllDataAndRecorded
        };

        ActivitySource.AddActivityListener(listener);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        listener.Dispose();
        disposed = true;
    }
}
