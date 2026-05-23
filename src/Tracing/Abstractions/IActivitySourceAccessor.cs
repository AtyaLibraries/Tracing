// <copyright file="IActivitySourceAccessor.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
namespace Atya.Diagnostics.Tracing.Abstractions;

/// <summary>
/// Provides access to a configured <see cref="ActivitySource"/> and helper methods for starting activities consistently.
/// </summary>
public interface IActivitySourceAccessor
{
    /// <summary>
    /// Gets the configured activity source.
    /// </summary>
    public ActivitySource ActivitySource { get; }

    /// <summary>
    /// Starts a new activity.
    /// </summary>
    /// <param name="name">The activity name.</param>
    /// <param name="kind">The activity kind.</param>
    /// <param name="parentContext">The parent context.</param>
    /// <param name="tags">Optional activity tags.</param>
    /// <param name="links">Optional activity links.</param>
    /// <returns>The started activity, or <see langword="null"/> when no listener is interested.</returns>
    public Activity? StartActivity(
        string name,
        ActivityKind kind = ActivityKind.Internal,
        ActivityContext parentContext = default,
        IEnumerable<KeyValuePair<string, object?>>? tags = null,
        IEnumerable<ActivityLink>? links = null);

    /// <summary>
    /// Starts a new internal activity.
    /// </summary>
    /// <param name="name">The activity name.</param>
    /// <param name="tags">Optional activity tags.</param>
    /// <returns>The started activity, or <see langword="null"/> when no listener is interested.</returns>
    public Activity? StartInternalActivity(
        string name,
        IEnumerable<KeyValuePair<string, object?>>? tags = null);

    /// <summary>
    /// Starts a new client activity.
    /// </summary>
    /// <param name="name">The activity name.</param>
    /// <param name="tags">Optional activity tags.</param>
    /// <returns>The started activity, or <see langword="null"/> when no listener is interested.</returns>
    public Activity? StartClientActivity(
        string name,
        IEnumerable<KeyValuePair<string, object?>>? tags = null);

    /// <summary>
    /// Starts a new server activity.
    /// </summary>
    /// <param name="name">The activity name.</param>
    /// <param name="tags">Optional activity tags.</param>
    /// <returns>The started activity, or <see langword="null"/> when no listener is interested.</returns>
    public Activity? StartServerActivity(
        string name,
        IEnumerable<KeyValuePair<string, object?>>? tags = null);

    /// <summary>
    /// Starts a new producer activity.
    /// </summary>
    /// <param name="name">The activity name.</param>
    /// <param name="tags">Optional activity tags.</param>
    /// <returns>The started activity, or <see langword="null"/> when no listener is interested.</returns>
    public Activity? StartProducerActivity(
        string name,
        IEnumerable<KeyValuePair<string, object?>>? tags = null);

    /// <summary>
    /// Starts a new consumer activity.
    /// </summary>
    /// <param name="name">The activity name.</param>
    /// <param name="tags">Optional activity tags.</param>
    /// <returns>The started activity, or <see langword="null"/> when no listener is interested.</returns>
    public Activity? StartConsumerActivity(
        string name,
        IEnumerable<KeyValuePair<string, object?>>? tags = null);
}
