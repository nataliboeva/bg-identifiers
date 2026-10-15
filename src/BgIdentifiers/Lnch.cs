using System.Diagnostics.CodeAnalysis;
using BgIdentifiers.Internal;

namespace BgIdentifiers;

/// <summary>
/// Validates LNCh (ЛНЧ, Личен номер на чужденец) - the 10-digit personal number
/// issued to foreigners residing in Bulgaria.
/// </summary>
/// <remarks>
/// Unlike an EGN, an LNCh carries no embedded information. The only check that can
/// be performed is the weighted mod 10 check digit in the last position.
/// </remarks>
public static class Lnch
{
    /// <summary>The number of digits in an LNCh.</summary>
    public const int Length = 10;

    private static ReadOnlySpan<int> Weights => [21, 19, 17, 13, 11, 9, 7, 3, 1];

    /// <summary>
    /// Checks whether <paramref name="lnch"/> is a valid LNCh: exactly 10 ASCII digits
    /// with a matching check digit.
    /// </summary>
    /// <param name="lnch">The value to check. It is not trimmed or normalized.</param>
    /// <returns><see langword="true"/> if the value is a valid LNCh; otherwise <see langword="false"/>.</returns>
    public static bool IsValid(ReadOnlySpan<char> lnch) =>
        lnch.Length == Length
        && Digits.AreAll(lnch)
        && Digits.ValueAt(lnch, 9) == CalculateCheckDigit(lnch);

    /// <inheritdoc cref="IsValid(ReadOnlySpan{char})"/>
    public static bool IsValid([NotNullWhen(true)] string? lnch) => lnch is not null && IsValid(lnch.AsSpan());

    private static int CalculateCheckDigit(ReadOnlySpan<char> lnch) => Digits.WeightedSum(lnch, Weights) % 10;
}
