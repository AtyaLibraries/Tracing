// <copyright file="ActivityExtensions.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
using Atya.Diagnostics.Tracing.Tags;
using Atya.Foundation.Guards;

namespace Atya.Diagnostics.Tracing.Extensions;

/// <summary>
/// Extension methods for enriching and marking <see cref="Activity"/> instances.
/// </summary>
public static class ActivityExtensions
{
    /// <summary>
    /// Sets a tag when the provided value is not <see langword="null"/>.
    /// </summary>
    /// <param name="activity">The activity.</param>
    /// <param name="name">The tag name.</param>
    /// <param name="value">The tag value.</param>
    /// <returns>The same activity instance.</returns>
    public static Activity SetTagIfNotNull(this Activity activity, string name, object? value)
    {
        _ = Guard.AgainstNull(activity);
        _ = Guard.AgainstNullOrWhiteSpace(name);

        if (value is not null)
        {
            _ = activity.SetTag(name, value);
        }

        return activity;
    }

    /// <summary>
    /// Sets the correlation id tag.
    /// </summary>
    /// <param name="activity">The activity.</param>
    /// <param name="correlationId">The correlation id.</param>
    /// <returns>The same activity instance.</returns>
    public static Activity SetCorrelationId(this Activity activity, string correlationId)
    {
        _ = Guard.AgainstNull(activity);
        _ = Guard.AgainstNullOrWhiteSpace(correlationId);

        _ = activity.SetTag(TracingTagNames.CorrelationId, correlationId);
        return activity;
    }

    /// <summary>
    /// Sets the tenant id tag.
    /// </summary>
    /// <param name="activity">The activity.</param>
    /// <param name="tenantId">The tenant id.</param>
    /// <returns>The same activity instance.</returns>
    public static Activity SetTenantId(this Activity activity, string tenantId)
    {
        _ = Guard.AgainstNull(activity);
        _ = Guard.AgainstNullOrWhiteSpace(tenantId);

        _ = activity.SetTag(TracingTagNames.TenantId, tenantId);
        return activity;
    }

    /// <summary>
    /// Sets the user id tag.
    /// </summary>
    /// <param name="activity">The activity.</param>
    /// <param name="userId">The user id.</param>
    /// <returns>The same activity instance.</returns>
    public static Activity SetUserId(this Activity activity, string userId)
    {
        _ = Guard.AgainstNull(activity);
        _ = Guard.AgainstNullOrWhiteSpace(userId);

        _ = activity.SetTag(TracingTagNames.UserId, userId);
        return activity;
    }

    /// <summary>
    /// Sets the operation name tag.
    /// </summary>
    /// <param name="activity">The activity.</param>
    /// <param name="operationName">The operation name.</param>
    /// <returns>The same activity instance.</returns>
    public static Activity SetOperationName(this Activity activity, string operationName)
    {
        _ = Guard.AgainstNull(activity);
        _ = Guard.AgainstNullOrWhiteSpace(operationName);

        _ = activity.SetTag(TracingTagNames.OperationName, operationName);
        return activity;
    }

    /// <summary>
    /// Sets the entity tags.
    /// </summary>
    /// <param name="activity">The activity.</param>
    /// <param name="entityType">The entity type.</param>
    /// <param name="entityId">The entity id.</param>
    /// <returns>The same activity instance.</returns>
    public static Activity SetEntity(this Activity activity, string entityType, object? entityId)
    {
        _ = Guard.AgainstNull(activity);
        _ = Guard.AgainstNullOrWhiteSpace(entityType);

        _ = activity.SetTag(TracingTagNames.EntityType, entityType);
        _ = activity.SetTagIfNotNull(TracingTagNames.EntityId, entityId);

        return activity;
    }

    /// <summary>
    /// Sets the dependency system tag.
    /// </summary>
    /// <param name="activity">The activity.</param>
    /// <param name="dependencySystem">The dependency system name.</param>
    /// <returns>The same activity instance.</returns>
    public static Activity SetDependencySystem(this Activity activity, string dependencySystem)
    {
        _ = Guard.AgainstNull(activity);
        _ = Guard.AgainstNullOrWhiteSpace(dependencySystem);

        _ = activity.SetTag(TracingTagNames.DependencySystem, dependencySystem);
        return activity;
    }

    /// <summary>
    /// Sets the messaging destination tag.
    /// </summary>
    /// <param name="activity">The activity.</param>
    /// <param name="destination">The destination name.</param>
    /// <returns>The same activity instance.</returns>
    public static Activity SetMessagingDestination(this Activity activity, string destination)
    {
        _ = Guard.AgainstNull(activity);
        _ = Guard.AgainstNullOrWhiteSpace(destination);

        _ = activity.SetTag(TracingTagNames.MessagingDestination, destination);
        return activity;
    }

    /// <summary>
    /// Marks the activity as successful.
    /// </summary>
    /// <param name="activity">The activity.</param>
    /// <returns>The same activity instance.</returns>
    public static Activity MarkSuccess(this Activity activity)
    {
        _ = Guard.AgainstNull(activity);

        _ = activity.SetStatus(ActivityStatusCode.Ok);
        _ = activity.SetTag(TracingTagNames.Outcome, "success");

        return activity;
    }

    /// <summary>
    /// Marks the activity as failed using the provided exception.
    /// </summary>
    /// <param name="activity">The activity.</param>
    /// <param name="exception">The exception.</param>
    /// <returns>The same activity instance.</returns>
    public static Activity MarkError(this Activity activity, Exception exception)
    {
        _ = Guard.AgainstNull(activity);
        _ = Guard.AgainstNull(exception);

        _ = activity.SetStatus(ActivityStatusCode.Error, exception.Message);
        _ = activity.SetTag(TracingTagNames.Outcome, "error");
        _ = activity.SetTag(TracingTagNames.ErrorType, exception.GetType().FullName);
        _ = activity.SetTagIfNotNull(TracingTagNames.ErrorMessage, string.IsNullOrWhiteSpace(exception.Message) ? null : exception.Message);

        return activity;
    }

    /// <summary>
    /// Marks the activity as failed using an explicit error type and optional message.
    /// </summary>
    /// <param name="activity">The activity.</param>
    /// <param name="errorType">The error type.</param>
    /// <param name="message">The optional error message.</param>
    /// <returns>The same activity instance.</returns>
    public static Activity MarkError(this Activity activity, string errorType, string? message = null)
    {
        _ = Guard.AgainstNull(activity);
        _ = Guard.AgainstNullOrWhiteSpace(errorType);

        _ = activity.SetStatus(ActivityStatusCode.Error, message);
        _ = activity.SetTag(TracingTagNames.Outcome, "error");
        _ = activity.SetTag(TracingTagNames.ErrorType, errorType);
        _ = activity.SetTagIfNotNull(TracingTagNames.ErrorMessage, string.IsNullOrWhiteSpace(message) ? null : message);

        return activity;
    }

    /// <summary>
    /// Sets a custom outcome value.
    /// </summary>
    /// <param name="activity">The activity.</param>
    /// <param name="outcome">The outcome value.</param>
    /// <returns>The same activity instance.</returns>
    public static Activity SetOutcome(this Activity activity, string outcome)
    {
        _ = Guard.AgainstNull(activity);
        _ = Guard.AgainstNullOrWhiteSpace(outcome);

        _ = activity.SetTag(TracingTagNames.Outcome, outcome);
        return activity;
    }
}
