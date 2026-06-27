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
    private readonly KeyValuePair<string, object?>[] _defaultTags;
    private readonly string? _serviceName;
    private readonly string? _serviceVersion;
    private bool _disposed;

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

        ActivitySource = new ActivitySource(tracingOptions.ActivitySourceName, tracingOptions.ActivitySourceVersion);
        _serviceName = NormalizeOptionalValue(tracingOptions.ServiceName);
        _serviceVersion = NormalizeOptionalValue(tracingOptions.ServiceVersion);
        _defaultTags = tracingOptions.DefaultTags.ToArray();
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
        ObjectDisposedException.ThrowIf(_disposed, this);
        _ = Guard.AgainstNullOrWhiteSpace(name);

        return ActivitySource.StartActivity(name, kind, parentContext, CreateActivityTags(tags), links);
    }

    /// <inheritdoc />
    public Activity? StartInternalActivity(
        string name,
        IEnumerable<KeyValuePair<string, object?>>? tags = null)
    {
        return StartActivity(name, ActivityKind.Internal, default, tags);
    }

    /// <inheritdoc />
    public Activity? StartClientActivity(
        string name,
        IEnumerable<KeyValuePair<string, object?>>? tags = null)
    {
        return StartActivity(name, ActivityKind.Client, default, tags);
    }

    /// <inheritdoc />
    public Activity? StartServerActivity(
        string name,
        IEnumerable<KeyValuePair<string, object?>>? tags = null)
    {
        return StartActivity(name, ActivityKind.Server, default, tags);
    }

    /// <inheritdoc />
    public Activity? StartProducerActivity(
        string name,
        IEnumerable<KeyValuePair<string, object?>>? tags = null)
    {
        return StartActivity(name, ActivityKind.Producer, default, tags);
    }

    /// <inheritdoc />
    public Activity? StartConsumerActivity(
        string name,
        IEnumerable<KeyValuePair<string, object?>>? tags = null)
    {
        return StartActivity(name, ActivityKind.Consumer, default, tags);
    }

    /// <summary>
    /// Disposes the owned <see cref="ActivitySource"/>.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        ActivitySource.Dispose();
        _disposed = true;
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
        if (_serviceName is null && _serviceVersion is null && _defaultTags.Length == 0)
        {
            return tags;
        }

        var activityTags = new Dictionary<string, object?>(StringComparer.Ordinal);

        AddIfNotNull(activityTags, Tags.TracingTagNames.ServiceName, _serviceName);
        AddIfNotNull(activityTags, Tags.TracingTagNames.ServiceVersion, _serviceVersion);

        foreach (var tag in _defaultTags)
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
