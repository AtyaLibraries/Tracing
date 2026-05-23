// <copyright file="TracingOptions.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
using Atya.Foundation.Guards;

namespace Atya.Diagnostics.Tracing.Options;

/// <summary>
/// Configures package-owned tracing services.
/// </summary>
public sealed class TracingOptions
{
    /// <summary>
    /// Gets or sets the activity source name used by the package-owned <see cref="ActivitySource"/>.
    /// </summary>
    public string ActivitySourceName { get; set; } = "Atya";

    /// <summary>
    /// Gets or sets the optional activity source version.
    /// </summary>
    public string? ActivitySourceVersion { get; set; }

    /// <summary>
    /// Gets or sets the logical service name added to package-created activities when configured.
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// Gets or sets the logical service version added to package-created activities when configured.
    /// </summary>
    public string? ServiceVersion { get; set; }

    /// <summary>
    /// Gets default tags added to package-created activities when the tag is not already present.
    /// </summary>
    public IDictionary<string, object?> DefaultTags { get; } = new Dictionary<string, object?>(StringComparer.Ordinal);

    /// <summary>
    /// Adds or replaces a default activity tag.
    /// </summary>
    /// <param name="name">The tag name.</param>
    /// <param name="value">The tag value.</param>
    /// <returns>The same options instance.</returns>
    public TracingOptions AddDefaultTag(string name, object? value)
    {
        _ = Guard.AgainstNullOrWhiteSpace(name);

        this.DefaultTags[name] = value;
        return this;
    }
}
