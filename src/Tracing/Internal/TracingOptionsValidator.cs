// <copyright file="TracingOptionsValidator.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
using Atya.Diagnostics.Tracing.Options;
using Microsoft.Extensions.Options;

namespace Atya.Diagnostics.Tracing.Internal;

internal sealed class TracingOptionsValidator : IValidateOptions<TracingOptions>
{
    public ValidateOptionsResult Validate(string? name, TracingOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ActivitySourceName))
        {
            failures.Add("TracingOptions.ActivitySourceName cannot be null or whitespace.");
        }

        if (options.ServiceName is not null && string.IsNullOrWhiteSpace(options.ServiceName))
        {
            failures.Add("TracingOptions.ServiceName cannot be whitespace.");
        }

        if (options.ServiceVersion is not null && string.IsNullOrWhiteSpace(options.ServiceVersion))
        {
            failures.Add("TracingOptions.ServiceVersion cannot be whitespace.");
        }

        if (options.DefaultTags.Keys.Any(string.IsNullOrWhiteSpace))
        {
            failures.Add("TracingOptions.DefaultTags cannot contain null, empty, or whitespace tag names.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
