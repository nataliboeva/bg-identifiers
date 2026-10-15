using System.Diagnostics.CodeAnalysis;
using BgIdentifiers.Internal;

namespace BgIdentifiers;

/// <summary>
/// Validates, parses and generates EGN (ЕГН, Единен граждански номер) - the
/// 10-digit personal identification number of Bulgarian citizens.
/// </summary>
/// <remarks>
/// Format: <c>YYMMDDSSGC</c>. The first six digits are the birth date, where 20 is
/// added to the month for people born in the 1800s and 40 for people born in the
/// 2000s. <c>SSG</c> is a sequence number whose last digit (<c>G</c>) is even for
/// men and odd for women, and <c>C</c> is a weighted mod 11 check digit.
/// </remarks>
public static class Egn
{
    /// <summary>The number of digits in an EGN.</summary>
    public const int Length = 10;

    private static ReadOnlySpan<int> Weights => [2, 4, 8, 5, 10, 9, 7, 3, 6];

    /// <summary>
    /// Checks whether <paramref name="egn"/> is a valid EGN: exactly 10 ASCII digits,
    /// a real calendar date and a matching check digit.
    /// </summary>
    /// <param name="egn">The value to check. It is not trimmed or normalized.</param>
    /// <returns><see langword="true"/> if the value is a valid EGN; otherwise <see langword="false"/>.</returns>
    public static bool IsValid(ReadOnlySpan<char> egn) => TryParse(egn, out _);

    /// <inheritdoc cref="IsValid(ReadOnlySpan{char})"/>
    public static bool IsValid([NotNullWhen(true)] string? egn) => egn is not null && IsValid(egn.AsSpan());

    /// <summary>
    /// Validates <paramref name="egn"/> and, if it is valid, decodes the birth date and gender.
    /// </summary>
    /// <param name="egn">The value to parse. It is not trimmed or normalized.</param>
    /// <param name="info">When this method returns <see langword="true"/>, the decoded information; otherwise <see langword="default"/>.</param>
    /// <returns><see langword="true"/> if the value is a valid EGN; otherwise <see langword="false"/>.</returns>
    public static bool TryParse(ReadOnlySpan<char> egn, out EgnInfo info)
    {
        info = default;

        if (egn.Length != Length || !Digits.AreAll(egn))
        {
            return false;
        }

        if (Digits.ValueAt(egn, 9) != CalculateCheckDigit(egn) || !TryGetBirthDate(egn, out var birthDate))
        {
            return false;
        }

        var gender = Digits.ValueAt(egn, 8) % 2 == 0 ? Gender.Male : Gender.Female;
        info = new EgnInfo(birthDate, gender);
        return true;
    }

    /// <inheritdoc cref="TryParse(ReadOnlySpan{char}, out EgnInfo)"/>
    public static bool TryParse([NotNullWhen(true)] string? egn, out EgnInfo info)
    {
        if (egn is null)
        {
            info = default;
            return false;
        }

        return TryParse(egn.AsSpan(), out info);
    }

    private static int CalculateCheckDigit(ReadOnlySpan<char> egn)
    {
        var remainder = Digits.WeightedSum(egn, Weights) % 11;
        return remainder == 10 ? 0 : remainder;
    }

    private static bool TryGetBirthDate(ReadOnlySpan<char> egn, out DateOnly birthDate)
    {
        birthDate = default;

        var year = Digits.TwoDigitNumber(egn, 0);
        var encodedMonth = Digits.TwoDigitNumber(egn, 2);
        var day = Digits.TwoDigitNumber(egn, 4);

        (int century, int month) = encodedMonth switch
        {
            >= 1 and <= 12 => (1900, encodedMonth),
            >= 21 and <= 32 => (1800, encodedMonth - 20),
            >= 41 and <= 52 => (2000, encodedMonth - 40),
            _ => (0, 0),
        };

        if (month == 0)
        {
            return false;
        }

        year += century;
        if (day < 1 || day > DateTime.DaysInMonth(year, month))
        {
            return false;
        }

        birthDate = new DateOnly(year, month, day);
        return true;
    }
}
