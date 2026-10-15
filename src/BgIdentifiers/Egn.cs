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

    /// <summary>The earliest birth date an EGN can encode.</summary>
    public static readonly DateOnly MinBirthDate = new(1800, 1, 1);

    /// <summary>The latest birth date an EGN can encode.</summary>
    public static readonly DateOnly MaxBirthDate = new(2099, 12, 31);

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

    /// <summary>
    /// Generates a random, valid EGN for the given birth date and gender. Intended for test data.
    /// </summary>
    /// <param name="birthDate">The birth date to encode, between <see cref="MinBirthDate"/> and <see cref="MaxBirthDate"/>.</param>
    /// <param name="gender">The gender to encode.</param>
    /// <param name="random">The random number generator to use, or <see langword="null"/> for <see cref="Random.Shared"/>.
    /// Pass a seeded instance for reproducible output.</param>
    /// <returns>A valid 10-digit EGN.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="birthDate"/> is outside the supported range, or <paramref name="gender"/> is not a defined value.
    /// </exception>
    public static string Generate(DateOnly birthDate, Gender gender, Random? random = null)
    {
        if (birthDate < MinBirthDate || birthDate > MaxBirthDate)
        {
            throw new ArgumentOutOfRangeException(nameof(birthDate), birthDate, "An EGN can only encode birth dates between 1800-01-01 and 2099-12-31.");
        }

        if (!Enum.IsDefined(gender))
        {
            throw new ArgumentOutOfRangeException(nameof(gender), gender, "Unknown gender value.");
        }

        random ??= Random.Shared;

        var monthOffset = birthDate.Year switch
        {
            < 1900 => 20,
            < 2000 => 0,
            _ => 40,
        };

        Span<char> egn = stackalloc char[Length];
        Digits.WriteTwoDigits(egn, 0, birthDate.Year % 100);
        Digits.WriteTwoDigits(egn, 2, birthDate.Month + monthOffset);
        Digits.WriteTwoDigits(egn, 4, birthDate.Day);
        Digits.FillRandom(egn.Slice(6, 2), random);

        // Even digits (0, 2, 4, 6, 8) mean male, odd digits (1, 3, 5, 7, 9) mean female.
        var genderDigit = (random.Next(5) * 2) + (gender == Gender.Male ? 0 : 1);
        egn[8] = Digits.ToChar(genderDigit);
        egn[9] = Digits.ToChar(CalculateCheckDigit(egn));

        return new string(egn);
    }

    private static int CalculateCheckDigit(ReadOnlySpan<char> egn)
    {
        var remainder = Digits.WeightedSum(egn, Weights) % 11;
        return remainder == 10 ? 0 : remainder;
    }

    private static bool TryGetBirthDate(ReadOnlySpan<char> egn, out DateOnly birthDate)
    {
        birthDate = default;

        var yearOfCentury = Digits.TwoDigitNumber(egn, 0);
        var encodedMonth = Digits.TwoDigitNumber(egn, 2);
        var day = Digits.TwoDigitNumber(egn, 4);

        var (century, month) = encodedMonth switch
        {
            >= 1 and <= 12 => (1900, encodedMonth),
            >= 21 and <= 32 => (1800, encodedMonth - 20),
            >= 41 and <= 52 => (2000, encodedMonth - 40),
            _ => (0, 0), // 00, 13-20, 33-40 and 53-99 are not valid months
        };

        if (month == 0)
        {
            return false;
        }

        var year = century + yearOfCentury;
        if (day < 1 || day > DateTime.DaysInMonth(year, month))
        {
            return false;
        }

        birthDate = new DateOnly(year, month, day);
        return true;
    }
}
