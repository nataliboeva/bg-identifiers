using System.Diagnostics.CodeAnalysis;
using BgIdentifiers.Internal;

namespace BgIdentifiers;

/// <summary>
/// Validates Bulgarian IBANs (International Bank Account Numbers) in electronic format,
/// for example <c>BG80BNBG96611020345678</c>.
/// </summary>
/// <remarks>
/// A Bulgarian IBAN has 22 characters: <c>BG</c>, two check digits, a 4-letter bank code
/// (the first four letters of the bank's BIC), a 4-digit branch code, a 2-digit account type
/// and an 8-character account number. The check digits are verified with the ISO 13616
/// mod 97 algorithm.
/// </remarks>
public static class BgIban
{
    /// <summary>The number of characters in a Bulgarian IBAN.</summary>
    public const int Length = 22;

    /// <summary>The ISO 3166 country code that every Bulgarian IBAN starts with.</summary>
    public const string CountryCode = "BG";

    /// <summary>
    /// Checks whether <paramref name="iban"/> is a valid Bulgarian IBAN in electronic format:
    /// 22 characters, upper case, without spaces.
    /// </summary>
    /// <param name="iban">The value to check. It is not trimmed or normalized, so the printed
    /// format with spaces (<c>BG80 BNBG 9661 1020 3456 78</c>) is rejected.</param>
    /// <returns><see langword="true"/> if the value is a valid Bulgarian IBAN; otherwise <see langword="false"/>.</returns>
    public static bool IsValid(ReadOnlySpan<char> iban) =>
        iban.Length == Length
        && iban.StartsWith(CountryCode, StringComparison.Ordinal)
        && HasValidStructure(iban)
        && HasValidCheckDigits(iban);

    /// <inheritdoc cref="IsValid(ReadOnlySpan{char})"/>
    public static bool IsValid([NotNullWhen(true)] string? iban) => iban is not null && IsValid(iban.AsSpan());

    /// <summary>
    /// Generates a random, structurally valid Bulgarian IBAN with correct check digits.
    /// The bank code is four random letters, so the result does not belong to a real bank or account.
    /// Intended for test data.
    /// </summary>
    /// <param name="random">The random number generator to use, or <see langword="null"/> for <see cref="Random.Shared"/>.
    /// Pass a seeded instance for reproducible output.</param>
    /// <returns>A valid 22-character IBAN in electronic format.</returns>
    public static string Generate(Random? random = null)
    {
        random ??= Random.Shared;

        Span<char> iban = stackalloc char[Length];
        CountryCode.AsSpan().CopyTo(iban);
        iban[2] = '0';
        iban[3] = '0';

        for (var i = 4; i < 8; i++)
        {
            iban[i] = (char)('A' + random.Next(26));
        }

        Digits.FillRandom(iban[8..], random);

        // With "00" as placeholder check digits, 98 - (value mod 97) gives the real ones.
        Digits.WriteTwoDigits(iban, 2, 98 - CalculateMod97(iban));

        return new string(iban);
    }

    // BBAN format from the SWIFT IBAN registry: 4!a 4!n 2!n 8!c
    private static bool HasValidStructure(ReadOnlySpan<char> iban) =>
        Digits.AreAll(iban.Slice(2, 2))
        && !iban.Slice(4, 4).ContainsAnyExceptInRange('A', 'Z')
        && Digits.AreAll(iban.Slice(8, 6))
        && IsUpperAlphanumeric(iban.Slice(14, 8));

    private static bool HasValidCheckDigits(ReadOnlySpan<char> iban)
    {
        // ISO 7064 MOD 97-10 only ever produces check digits between 02 and 98.
        var checkDigits = Digits.TwoDigitNumber(iban, 2);
        return checkDigits is >= 2 and <= 98 && CalculateMod97(iban) == 1;
    }

    /// <summary>
    /// Moves the first four characters to the end, replaces each letter with a two-digit number
    /// (A = 10 ... Z = 35) and returns the resulting integer mod 97. The number is processed
    /// one character at a time, so it never needs to be materialized.
    /// </summary>
    private static int CalculateMod97(ReadOnlySpan<char> iban)
    {
        var remainder = 0;
        for (var i = 0; i < iban.Length; i++)
        {
            var c = iban[(i + 4) % iban.Length];
            remainder = char.IsAsciiDigit(c)
                ? ((remainder * 10) + (c - '0')) % 97
                : ((remainder * 100) + (c - 'A' + 10)) % 97;
        }

        return remainder;
    }

    private static bool IsUpperAlphanumeric(ReadOnlySpan<char> value)
    {
        foreach (var c in value)
        {
            if (!char.IsAsciiDigit(c) && !char.IsAsciiLetterUpper(c))
            {
                return false;
            }
        }

        return true;
    }
}
