// <copyright file="CorrelationIdPolicy.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
namespace Atya.Diagnostics.Tracing.Correlation;

/// <summary>
/// Decides which correlation ids are acceptable and generates new ones.
/// </summary>
public sealed class CorrelationIdPolicy
{
    private int maxLength = 128;
    private Func<char, bool> isAllowedChar = DefaultIsAllowedChar;

    /// <summary>
    /// Gets or sets the maximum accepted length. The default is 128.
    /// </summary>
    public int MaxLength
    {
        get => maxLength;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            maxLength = value;
        }
    }

    /// <summary>
    /// Gets or sets the predicate for allowed characters. The default allows ASCII letters, digits and <c>- _ . :</c>.
    /// </summary>
    public Func<char, bool> IsAllowedChar
    {
        get => isAllowedChar;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            isAllowedChar = value;
        }
    }

    /// <summary>
    /// Generates a new correlation id.
    /// </summary>
    /// <returns>A 32-character lowercase hexadecimal GUID.</returns>
    public static string Generate() => Guid.NewGuid().ToString("N");

    /// <summary>
    /// Resolves the single acceptable id from inbound header values.
    /// </summary>
    /// <param name="inboundValues">The raw header values.</param>
    /// <returns>
    /// The id, or <see langword="null"/> when there is no value, a value is too long or contains a disallowed character,
    /// or several distinct values were supplied.
    /// </returns>
    public string? Resolve(IEnumerable<string?> inboundValues)
    {
        ArgumentNullException.ThrowIfNull(inboundValues);

        string? found = null;
        foreach (var raw in inboundValues)
        {
            var value = raw?.Trim();
            if (string.IsNullOrEmpty(value))
            {
                continue;
            }

            if (found is null)
            {
                found = value;
            }
            else if (!string.Equals(found, value, StringComparison.Ordinal))
            {
                return null;
            }
        }

        return found is not null && IsValid(found) ? found : null;
    }

    /// <summary>
    /// Returns whether the id satisfies the policy.
    /// </summary>
    /// <param name="id">The id to check.</param>
    /// <returns><see langword="true"/> when the id is acceptable.</returns>
    public bool IsValid(string? id)
    {
        if (string.IsNullOrEmpty(id) || id.Length > maxLength)
        {
            return false;
        }

        foreach (var c in id)
        {
            if (!isAllowedChar(c))
            {
                return false;
            }
        }

        return true;
    }

    private static bool DefaultIsAllowedChar(char c) =>
        c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or (>= '0' and <= '9') or '-' or '_' or '.' or ':';
}
