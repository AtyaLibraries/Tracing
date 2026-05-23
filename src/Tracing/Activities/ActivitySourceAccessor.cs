// <copyright file="ActivitySourceAccessor.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
using Atya.Diagnostics.Tracing.Abstractions;
using Atya.Diagnostics.Tracing.Options;
using Atya.Foundation.Guards;
using Microsoft.Extensions.Options;

namespace Atya.Diagnostics.Tracing.Activities;

/// <summary>
/// Default implementation of <see cref="IActivitySourceAccessor"/>.
/// </summary>
public sealed class ActivitySourceAccessor : IActivitySourceAccessor, IDisposable
{
    private readonly KeyValuePair<string, object?>[] defaultTags;
    private readonly string? serviceName;
    private readonly string? serviceVersion;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActivitySourceAccessor"/> class.
    /// </summary>
    /// <param name="options">Tracing options.</param>
    public ActivitySourceAccessor(IOptions<TracingOptions> options)
    {
        _ = Guard.AgainstNull(options);

        var tracingOptions = options.Value;
        _ = Guard.AgainstNullOrWhiteSpace(tracingOptions.ActivitySourceName, nameof(tracingOptions.ActivitySourceName));
        ValidateOptionalValue(tracingOptions.ServiceName, nameof(tracingOptions.ServiceName));
        ValidateOptionalValue(tracingOptions.ServiceVersion, nameof(tracingOptions.ServiceVersion));
        ValidateDefaultTagNames(tracingOptions.DefaultTags);

        this.ActivitySource = new ActivitySource(tracingOptions.ActivitySourceName, tracingOptions.ActivitySourceVersion);
        this.serviceName = NormalizeOptionalValue(tracingOptions.ServiceName);
        this.serviceVersion = NormalizeOptionalValue(tracingOptions.ServiceVersion);
        this.defaultTags = tracingOptions.DefaultTags.ToArray();
    }

    /// <inheritdoc />
    public ActivitySource ActivitySource { get; }

    /// <inheritdoc />
    public Activity? StartActivity(
        string name,
        ActivityKind kind = ActivityKind.Internal,
        ActivityContext parentContext = default,
        IEnumerable<KeyValuePair<string, object?>>? tags = null,
        IEnumerable<ActivityLink>? links = null)
    {
        ObjectDisposedException.ThrowIf(this.disposed, this);
        _ = Guard.AgainstNullOrWhiteSpace(name);

        return this.ActivitySource.StartActivity(name, kind, parentContext, this.CreateActivityTags(tags), links);
    }

    /// <inheritdoc />
    public Activity? StartInternalActivity(
        string name,
        IEnumerable<KeyValuePair<string, object?>>? tags = null)
    {
        return this.StartActivity(name, ActivityKind.Internal, default, tags);
    }

    /// <inheritdoc />
    public Activity? StartClientActivity(
        string name,
        IEnumerable<KeyValuePair<string, object?>>? tags = null)
    {
        return this.StartActivity(name, ActivityKind.Client, default, tags);
    }

    /// <inheritdoc />
    public Activity? StartServerActivity(
        string name,
        IEnumerable<KeyValuePair<string, object?>>? tags = null)
    {
        return this.StartActivity(name, ActivityKind.Server, default, tags);
    }

    /// <inheritdoc />
    public Activity? StartProducerActivity(
        string name,
        IEnumerable<KeyValuePair<string, object?>>? tags = null)
    {
        return this.StartActivity(name, ActivityKind.Producer, default, tags);
    }

    /// <inheritdoc />
    public Activity? StartConsumerActivity(
        string name,
        IEnumerable<KeyValuePair<string, object?>>? tags = null)
    {
        return this.StartActivity(name, ActivityKind.Consumer, default, tags);
    }

    /// <summary>
    /// Disposes the owned <see cref="ActivitySource"/>.
    /// </summary>
    public void Dispose()
    {
        if (this.disposed)
        {
            return;
        }

        this.ActivitySource.Dispose();
        this.disposed = true;
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static void ValidateOptionalValue(string? value, string paramName)
    {
        if (value is not null)
        {
            _ = Guard.AgainstNullOrWhiteSpace(value, paramName);
        }
    }

    private static void ValidateDefaultTagNames(IEnumerable<KeyValuePair<string, object?>> defaultTags)
    {
        foreach (var tag in defaultTags)
        {
            _ = Guard.AgainstNullOrWhiteSpace(tag.Key, nameof(TracingOptions.DefaultTags));
        }
    }

    private static void AddIfNotNull(Dictionary<string, object?> tags, string name, object? value)
    {
        if (value is not null)
        {
            tags[name] = value;
        }
    }

    private IEnumerable<KeyValuePair<string, object?>>? CreateActivityTags(IEnumerable<KeyValuePair<string, object?>>? tags)
    {
        if (this.serviceName is null && this.serviceVersion is null && this.defaultTags.Length == 0)
        {
            return tags;
        }

        var activityTags = new Dictionary<string, object?>(StringComparer.Ordinal);

        AddIfNotNull(activityTags, Tags.TracingTagNames.ServiceName, this.serviceName);
        AddIfNotNull(activityTags, Tags.TracingTagNames.ServiceVersion, this.serviceVersion);

        foreach (var tag in this.defaultTags)
        {
            AddIfNotNull(activityTags, tag.Key, tag.Value);
        }

        if (tags is not null)
        {
            foreach (var tag in tags)
            {
                activityTags[tag.Key] = tag.Value;
            }
        }

        return activityTags;
    }
}
