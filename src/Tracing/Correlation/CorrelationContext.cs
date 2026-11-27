// <copyright file="CorrelationContext.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
using Atya.Diagnostics.Tracing.Extensions;

namespace Atya.Diagnostics.Tracing.Correlation;

/// <summary>
/// Ambient carrier for the current correlation id. Uses an <see cref="AsyncLocal{T}"/>; it never uses Activity baggage
/// and never creates an <see cref="Activity"/>.
/// </summary>
public static class CorrelationContext
{
    private static readonly AsyncLocal<string?> Value = new();

    /// <summary>
    /// Gets the correlation id for the current logical flow, or <see langword="null"/>.
    /// </summary>
    public static string? Current => Value.Value;

    /// <summary>
    /// Makes <paramref name="id"/> current and tags <see cref="Activity.Current"/> with it when one exists.
    /// </summary>
    /// <param name="id">The correlation id.</param>
    /// <returns>A scope that restores the previous id when disposed.</returns>
    public static IDisposable Push(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var previous = Value.Value;
        Value.Value = id;
        _ = Activity.Current?.SetCorrelationId(id);
        return new Scope(previous);
    }

    private sealed class Scope(string? previous) : IDisposable
    {
        private int disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) == 0)
            {
                Value.Value = previous;
            }
        }
    }
}
