using System.Diagnostics.CodeAnalysis;
using BgIdentifiers.Internal;

namespace BgIdentifiers;

/// <summary>
/// Validates EIK (ЕИК, Единен идентификационен код), also known as the BULSTAT code -
/// the identifier of Bulgarian companies, sole traders, institutions and their branches.
/// </summary>
/// <remarks>
/// <para>
/// A 9-digit code identifies a legal entity. Its ninth digit is a check digit calculated
/// with the weights 1-8 mod 11, falling back to the weights 3-10 when the remainder is 10.
/// </para>
/// <para>
/// A 13-digit code identifies a branch or division. The first nine digits are the parent's
/// valid EIK and the 13th digit checks digits 9-12 with the weights 2, 7, 3, 5, falling back
/// to 4, 9, 5, 7. In both cases a remainder that is still 10 becomes 0.
/// </para>
/// </remarks>
public static class Eik
{
    /// <summary>The number of digits in the EIK of a legal entity.</summary>
    public const int Length = 9;

    /// <summary>The number of digits in the EIK of a branch or division.</summary>
    public const int BranchLength = 13;

    private static ReadOnlySpan<int> Weights => [1, 2, 3, 4, 5, 6, 7, 8];
    private static ReadOnlySpan<int> FallbackWeights => [3, 4, 5, 6, 7, 8, 9, 10];
    private static ReadOnlySpan<int> BranchWeights => [2, 7, 3, 5];
    private static ReadOnlySpan<int> BranchFallbackWeights => [4, 9, 5, 7];

    /// <summary>
    /// Checks whether <paramref name="eik"/> is a valid 9-digit or 13-digit EIK/BULSTAT code.
    /// </summary>
    /// <param name="eik">The value to check. It is not trimmed or normalized.</param>
    /// <returns><see langword="true"/> if the value is a valid EIK; otherwise <see langword="false"/>.</returns>
    public static bool IsValid(ReadOnlySpan<char> eik)
    {
        if (eik.Length is not (Length or BranchLength) || !Digits.AreAll(eik))
        {
            return false;
        }

        if (Digits.ValueAt(eik, 8) != CalculateCheckDigit(eik[..8], Weights, FallbackWeights))
        {
            return false;
        }

        return eik.Length == Length
            || Digits.ValueAt(eik, 12) == CalculateCheckDigit(eik.Slice(8, 4), BranchWeights, BranchFallbackWeights);
    }

    /// <inheritdoc cref="IsValid(ReadOnlySpan{char})"/>
    public static bool IsValid([NotNullWhen(true)] string? eik) => eik is not null && IsValid(eik.AsSpan());

    private static int CalculateCheckDigit(ReadOnlySpan<char> digits, ReadOnlySpan<int> weights, ReadOnlySpan<int> fallbackWeights)
    {
        var remainder = Digits.WeightedSum(digits, weights) % 11;
        if (remainder == 10)
        {
            remainder = Digits.WeightedSum(digits, fallbackWeights) % 11;
        }

        return remainder == 10 ? 0 : remainder;
    }
}
