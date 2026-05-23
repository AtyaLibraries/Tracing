// <copyright file="TracingTagNames.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
namespace Atya.Diagnostics.Tracing.Tags;

/// <summary>
/// Well-known tracing tag names used by this package.
/// </summary>
public static class TracingTagNames
{
    public const string CorrelationId = "correlation.id";
    public const string TenantId = "tenant.id";
    public const string UserId = "user.id";
    public const string OperationName = "operation.name";
    public const string EntityType = "entity.type";
    public const string EntityId = "entity.id";
    public const string Outcome = "outcome";
    public const string ErrorType = "error.type";
    public const string ErrorMessage = "error.message";
    public const string DependencySystem = "dependency.system";
    public const string MessagingDestination = "messaging.destination";
    public const string ServiceName = "service.name";
    public const string ServiceVersion = "service.version";
}
