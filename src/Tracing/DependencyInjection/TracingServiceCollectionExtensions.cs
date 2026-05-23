// <copyright file="TracingServiceCollectionExtensions.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
using Atya.Diagnostics.Tracing.Abstractions;
using Atya.Diagnostics.Tracing.Activities;
using Atya.Diagnostics.Tracing.Internal;
using Atya.Diagnostics.Tracing.Options;
using Atya.Foundation.Guards;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Atya.Diagnostics.Tracing.DependencyInjection;

/// <summary>
/// Service registration extensions for Atya tracing services.
/// </summary>
public static class TracingServiceCollectionExtensions
{
    /// <summary>
    /// Registers Atya tracing services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional options configuration.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddAtyaTracing(
        this IServiceCollection services,
        Action<TracingOptions>? configure = null)
    {
        _ = Guard.AgainstNull(services);

        _ = services.AddOptions<TracingOptions>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            Microsoft.Extensions.Options.IValidateOptions<TracingOptions>,
            TracingOptionsValidator>());

        if (configure is not null)
        {
            _ = services.Configure(configure);
        }

        services.TryAddSingleton<IActivitySourceAccessor, ActivitySourceAccessor>();

        return services;
    }
}
