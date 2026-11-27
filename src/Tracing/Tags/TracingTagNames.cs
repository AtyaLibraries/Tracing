// <copyright file="TracingTagNames.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
namespace Atya.Diagnostics.Tracing.Tags;

/// <summary>
/// Well-known tracing tag names used by this package.
/// </summary>
public static class TracingTagNames
{
    /// <summary>
    /// The correlation identifier tag name.
    /// </summary>
    public const string CorrelationId = "correlation.id";

    /// <summary>
    /// The tenant identifier tag name.
    /// </summary>
    public const string TenantId = "tenant.id";

    /// <summary>
    /// The user identifier tag name.
    /// </summary>
    public const string UserId = "user.id";

    /// <summary>
    /// The operation name tag.
    /// </summary>
    public const string OperationName = "operation.name";

    /// <summary>
    /// The entity type tag name.
    /// </summary>
    public const string EntityType = "entity.type";

    /// <summary>
    /// The entity identifier tag name.
    /// </summary>
    public const string EntityId = "entity.id";

    /// <summary>
    /// The operation outcome tag name.
    /// </summary>
    public const string Outcome = "outcome";

    /// <summary>
    /// The error type tag name.
    /// </summary>
    public const string ErrorType = "error.type";

    /// <summary>
    /// The error message tag name.
    /// </summary>
    public const string ErrorMessage = "error.message";

    /// <summary>
    /// The dependency system tag name.
    /// </summary>
    public const string DependencySystem = "dependency.system";

    /// <summary>
    /// The messaging destination tag name.
    /// </summary>
    public const string MessagingDestination = "messaging.destination";

    /// <summary>
    /// The service name tag name.
    /// </summary>
    public const string ServiceName = "service.name";

    /// <summary>
    /// The service version tag name.
    /// </summary>
    public const string ServiceVersion = "service.version";
}
