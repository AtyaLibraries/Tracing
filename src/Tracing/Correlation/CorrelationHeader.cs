// <copyright file="CorrelationHeader.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
namespace Atya.Diagnostics.Tracing.Correlation;

/// <summary>
/// Owns the default correlation id header name shared by Atya packages.
/// </summary>
public static class CorrelationHeader
{
    /// <summary>
    /// The default correlation id header name.
    /// </summary>
    public const string DefaultName = "X-Correlation-ID";
}
